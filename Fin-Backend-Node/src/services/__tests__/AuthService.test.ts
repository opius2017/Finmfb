import { AuthService } from '../AuthService';
import { RepositoryFactory } from '@repositories/index';
import bcrypt from 'bcrypt';

// Mock dependencies
jest.mock('@repositories/index');
jest.mock('@config/database');
jest.mock('bcrypt');

describe('AuthService', () => {
  let authService: AuthService;
  let mockUserRepository: any;
  let mockPrisma: any;

  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();

    // Setup mock user repository
    mockUserRepository = {
      findByEmail: jest.fn(),
      updateLastLogin: jest.fn(),
      incrementFailedLogin: jest.fn(),
      lockAccount: jest.fn(),
    };

    // Setup mock prisma
    mockPrisma = {
      session: {
        create: jest.fn(),
        findUnique: jest.fn(),
        delete: jest.fn(),
        deleteMany: jest.fn(),
        update: jest.fn(),
      },
    };

    (RepositoryFactory.getUserRepository as jest.Mock).mockReturnValue(
      mockUserRepository
    );

    authService = new AuthService();
    (authService as any).prisma = mockPrisma;
  });

  describe('login', () => {
    it('should successfully login with valid credentials', async () => {
      const mockUser = {
        id: 'user-123',
        email: 'test@example.com',
        passwordHash: 'hashed-password',
        roleId: 'role-123',
        isActive: true,
        lockedUntil: null,
        failedLoginCount: 0,
      };

      mockUserRepository.findByEmail.mockResolvedValue(mockUser);
      (bcrypt.compare as jest.Mock).mockResolvedValue(true);
      mockPrisma.session.create.mockResolvedValue({
        id: 'session-123',
        userId: mockUser.id,
        refreshToken: 'temp-token',
        expiresAt: new Date(),
      });

      const result = await authService.login({
        email: 'test@example.com',
        password: 'password123',
      });

      expect(result).toHaveProperty('accessToken');
      expect(result).toHaveProperty('refreshToken');
      expect(result).toHaveProperty('expiresIn');
      expect(mockUserRepository.updateLastLogin).toHaveBeenCalledWith(mockUser.id);
    });

    it('should throw error for invalid email', async () => {
      mockUserRepository.findByEmail.mockResolvedValue(null);

      await expect(
        authService.login({
          email: 'invalid@example.com',
          password: 'password123',
        })
      ).rejects.toThrow('Invalid email or password');
    });

    it('should throw error for inactive user', async () => {
      const mockUser = {
        id: 'user-123',
        email: 'test@example.com',
        passwordHash: 'hashed-password',
        isActive: false,
      };

      mockUserRepository.findByEmail.mockResolvedValue(mockUser);

      await expect(
        authService.login({
          email: 'test@example.com',
          password: 'password123',
        })
      ).rejects.toThrow('Account is inactive');
    });

    it('should throw error for locked account', async () => {
      const futureDate = new Date();
      futureDate.setMinutes(futureDate.getMinutes() + 30);

      const mockUser = {
        id: 'user-123',
        email: 'test@example.com',
        passwordHash: 'hashed-password',
        isActive: true,
        lockedUntil: futureDate,
      };

      mockUserRepository.findByEmail.mockResolvedValue(mockUser);

      await expect(
        authService.login({
          email: 'test@example.com',
          password: 'password123',
        })
      ).rejects.toThrow(/Account is locked/);
    });

    it('should increment failed login count on wrong password', async () => {
      const mockUser = {
        id: 'user-123',
        email: 'test@example.com',
        passwordHash: 'hashed-password',
        isActive: true,
        lockedUntil: null,
        failedLoginCount: 0,
      };

      mockUserRepository.findByEmail.mockResolvedValue(mockUser);
      (bcrypt.compare as jest.Mock).mockResolvedValue(false);

      await expect(
        authService.login({
          email: 'test@example.com',
          password: 'wrong-password',
        })
      ).rejects.toThrow('Invalid email or password');

      expect(mockUserRepository.incrementFailedLogin).toHaveBeenCalledWith(
        mockUser.id
      );
    });
  });

  describe('logout', () => {
    it('should delete specific session when refresh token provided', async () => {
      const userId = 'user-123';
      const refreshToken = 'refresh-token-123';

      mockPrisma.session.deleteMany.mockResolvedValue({ count: 1 });

      await authService.logout(userId, refreshToken);

      expect(mockPrisma.session.deleteMany).toHaveBeenCalledWith({
        where: {
          userId,
          refreshToken,
        },
      });
    });

    it('should delete all sessions when no refresh token provided', async () => {
      const userId = 'user-123';

      mockPrisma.session.deleteMany.mockResolvedValue({ count: 2 });

      await authService.logout(userId);

      expect(mockPrisma.session.deleteMany).toHaveBeenCalledWith({
        where: { userId },
      });
    });
  });

  describe('logoutAll', () => {
    it('should delete all user sessions', async () => {
      const userId = 'user-123';

      mockPrisma.session.deleteMany.mockResolvedValue({ count: 3 });

      await authService.logoutAll(userId);

      expect(mockPrisma.session.deleteMany).toHaveBeenCalledWith({
        where: { userId },
      });
    });
  });
});
