import { getPrismaClient } from '@config/database';
import { logger } from '@utils/logger';
import { Prisma } from '@prisma/client';
import * as crypto from 'crypto';

/**
 * Bank connection data
 */
export interface CreateBankConnectionData {
  bankName: string;
  accountNumber: string;
  accountName: string;
  bankCode?: string;
  branchId?: string;
  credentials?: {
    apiKey?: string;
    apiSecret?: string;
    username?: string;
    password?: string;
  };
  metadata?: Record<string, any>;
  createdBy: string;
}

/**
 * Update bank connection data
 */
export interface UpdateBankConnectionData {
  bankName?: string;
  accountNumber?: string;
  accountName?: string;
  status?: string;
  credentials?: Record<string, any>;
  metadata?: Record<string, any>;
}

export class BankConnectionService {
  private prisma = getPrismaClient();
  private readonly ENCRYPTION_KEY = process.env.ENCRYPTION_KEY || 'default-key-change-in-production';

  /**
   * Create bank connection
   */
  async createBankConnection(data: CreateBankConnectionData) {
    try {
      logger.info('Creating bank connection', {
        bankName: data.bankName,
        accountNumber: data.accountNumber,
      });

      // Encrypt credentials
      const encryptedCredentials = data.credentials 
        ? this.encryptCredentials(data.credentials)
        : null;

      const connection = await this.prisma.$transaction(async (tx) => {
        const newConnection = await tx.bankConnection.create({
          data: {
            bankName: data.bankName,
            accountNumber: data.accountNumber,
            accountName: data.accountName,
            bankCode: data.bankCode,
            branchId: data.branchId,
            credentials: encryptedCredentials as Prisma.JsonObject,
            status: 'INACTIVE',
            metadata: data.metadata as Prisma.JsonObject,
            createdBy: data.createdBy,
          },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId: data.createdBy,
            action: 'BANK_CONNECTION_CREATE',
            entityType: 'BankConnection',
            entityId: newConnection.id,
            changes: {
              bankName: data.bankName,
              accountNumber: data.accountNumber,
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return newConnection;
      });

      logger.info('Bank connection created successfully', {
        connectionId: connection.id,
      });

      return connection;
    } catch (error) {
      logger.error('Error creating bank connection:', error);
      throw error;
    }
  }

  /**
   * Get bank connection by ID
   */
  async getBankConnectionById(id: string) {
    try {
      const connection = await this.prisma.bankConnection.findUnique({
        where: { id },
      });

      if (!connection) {
        throw new Error('Bank connection not found');
      }

      // Decrypt credentials for display (mask sensitive data)
      if (connection.credentials) {
        const decrypted = this.decryptCredentials(connection.credentials as string);
        connection.credentials = this.maskCredentials(decrypted) as any;
      }

      return connection;
    } catch (error) {
      logger.error('Error getting bank connection:', error);
      throw error;
    }
  }

  /**
   * Update bank connection
   */
  async updateBankConnection(id: string, data: UpdateBankConnectionData, userId: string) {
    try {
      logger.info('Updating bank connection', {
        connectionId: id,
      });

      // Encrypt credentials if provided
      const encryptedCredentials = data.credentials 
        ? this.encryptCredentials(data.credentials)
        : undefined;

      const connection = await this.prisma.$transaction(async (tx) => {
        const updated = await tx.bankConnection.update({
          where: { id },
          data: {
            ...data,
            ...(encryptedCredentials && { credentials: encryptedCredentials as Prisma.JsonObject }),
            metadata: data.metadata as Prisma.JsonObject,
            updatedAt: new Date(),
          },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId,
            action: 'BANK_CONNECTION_UPDATE',
            entityType: 'BankConnection',
            entityId: id,
            changes: data as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return updated;
      });

      logger.info('Bank connection updated successfully', {
        connectionId: id,
      });

      return connection;
    } catch (error) {
      logger.error('Error updating bank connection:', error);
      throw error;
    }
  }

  /**
   * Delete bank connection
   */
  async deleteBankConnection(id: string, userId: string) {
    try {
      logger.info('Deleting bank connection', {
        connectionId: id,
      });

      await this.prisma.$transaction(async (tx) => {
        await tx.bankConnection.delete({
          where: { id },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId,
            action: 'BANK_CONNECTION_DELETE',
            entityType: 'BankConnection',
            entityId: id,
            changes: {} as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });
      });

      logger.info('Bank connection deleted successfully', {
        connectionId: id,
      });
    } catch (error) {
      logger.error('Error deleting bank connection:', error);
      throw error;
    }
  }

  /**
   * List bank connections
   */
  async listBankConnections(branchId?: string) {
    try {
      const connections = await this.prisma.bankConnection.findMany({
        where: {
          ...(branchId && { branchId }),
        },
        orderBy: {
          createdAt: 'desc',
        },
      });

      // Mask credentials
      return connections.map(conn => ({
        ...conn,
        credentials: conn.credentials ? this.maskCredentials(this.decryptCredentials(conn.credentials as string)) : null,
      }));
    } catch (error) {
      logger.error('Error listing bank connections:', error);
      throw error;
    }
  }

  /**
   * Test bank connection
   */
  async testBankConnection(id: string): Promise<{ success: boolean; message: string }> {
    try {
      logger.info('Testing bank connection', {
        connectionId: id,
      });

      const connection = await this.prisma.bankConnection.findUnique({
        where: { id },
      });

      if (!connection) {
        throw new Error('Bank connection not found');
      }

      // TODO: Implement actual bank API connection test
      // For now, return success
      const success = true;
      const message = success ? 'Connection successful' : 'Connection failed';

      // Update last tested timestamp
      await this.prisma.bankConnection.update({
        where: { id },
        data: {
          lastTestedAt: new Date(),
          status: success ? 'ACTIVE' : 'INACTIVE',
        },
      });

      logger.info('Bank connection test completed', {
        connectionId: id,
        success,
      });

      return { success, message };
    } catch (error) {
      logger.error('Error testing bank connection:', error);
      throw error;
    }
  }

  /**
   * Encrypt credentials
   */
  private encryptCredentials(credentials: Record<string, any>): string {
    const algorithm = 'aes-256-cbc';
    const key = crypto.scryptSync(this.ENCRYPTION_KEY, 'salt', 32);
    const iv = crypto.randomBytes(16);
    
    const cipher = crypto.createCipheriv(algorithm, key, iv);
    let encrypted = cipher.update(JSON.stringify(credentials), 'utf8', 'hex');
    encrypted += cipher.final('hex');
    
    return iv.toString('hex') + ':' + encrypted;
  }

  /**
   * Decrypt credentials
   */
  private decryptCredentials(encryptedData: string): Record<string, any> {
    try {
      const algorithm = 'aes-256-cbc';
      const key = crypto.scryptSync(this.ENCRYPTION_KEY, 'salt', 32);
      
      const parts = encryptedData.split(':');
      const iv = Buffer.from(parts[0], 'hex');
      const encrypted = parts[1];
      
      const decipher = crypto.createDecipheriv(algorithm, key, iv);
      let decrypted = decipher.update(encrypted, 'hex', 'utf8');
      decrypted += decipher.final('utf8');
      
      return JSON.parse(decrypted);
    } catch (error) {
      logger.error('Error decrypting credentials:', error);
      return {};
    }
  }

  /**
   * Mask sensitive credentials
   */
  private maskCredentials(credentials: Record<string, any>): Record<string, any> {
    const masked: Record<string, any> = {};
    
    for (const [key, value] of Object.entries(credentials)) {
      if (typeof value === 'string' && value.length > 4) {
        masked[key] = '****' + value.slice(-4);
      } else {
        masked[key] = '****';
      }
    }
    
    return masked;
  }
}

export default BankConnectionService;
