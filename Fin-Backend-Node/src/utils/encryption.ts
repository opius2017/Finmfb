import crypto from 'crypto';
import { createBadRequestError } from '@middleware/errorHandler';

const ALGORITHM = 'aes-256-gcm';
const KEY_LENGTH = 32; // 256 bits
const IV_LENGTH = 16; // 128 bits
const AUTH_TAG_LENGTH = 16; // 128 bits
const SALT_LENGTH = 64;

/**
 * Encryption utility class
 */
export class EncryptionService {
  private encryptionKey: Buffer;

  constructor() {
    const key = process.env.ENCRYPTION_KEY;
    if (!key || key.length < 32) {
      throw new Error('ENCRYPTION_KEY must be at least 32 characters long');
    }
    this.encryptionKey = Buffer.from(key.slice(0, 32), 'utf-8');
  }

  /**
   * Encrypt data
   */
  encrypt(data: string): string {
    try {
      const iv = crypto.randomBytes(IV_LENGTH);
      const cipher = crypto.createCipheriv(ALGORITHM, this.encryptionKey, iv);

      let encrypted = cipher.update(data, 'utf8', 'hex');
      encrypted += cipher.final('hex');

      const authTag = cipher.getAuthTag();

      // Combine IV + AuthTag + Encrypted Data
      return iv.toString('hex') + authTag.toString('hex') + encrypted;
    } catch (error) {
      throw createBadRequestError('Encryption failed');
    }
  }

  /**
   * Decrypt data
   */
  decrypt(encryptedData: string): string {
    try {
      // Extract IV, AuthTag, and encrypted data
      const iv = Buffer.from(encryptedData.slice(0, IV_LENGTH * 2), 'hex');
      const authTag = Buffer.from(
        encryptedData.slice(IV_LENGTH * 2, (IV_LENGTH + AUTH_TAG_LENGTH) * 2),
        'hex'
      );
      const encrypted = encryptedData.slice((IV_LENGTH + AUTH_TAG_LENGTH) * 2);

      const decipher = crypto.createDecipheriv(ALGORITHM, this.encryptionKey, iv);
      decipher.setAuthTag(authTag);

      let decrypted = decipher.update(encrypted, 'hex', 'utf8');
      decrypted += decipher.final('utf8');

      return decrypted;
    } catch (error) {
      throw createBadRequestError('Decryption failed');
    }
  }

  /**
   * Hash data (one-way)
   */
  hash(data: string): string {
    return crypto.createHash('sha256').update(data).digest('hex');
  }

  /**
   * Generate random token
   */
  generateToken(length: number = 32): string {
    return crypto.randomBytes(length).toString('hex');
  }

  /**
   * Generate secure random string
   */
  generateSecureRandom(length: number = 16): string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    const randomBytes = crypto.randomBytes(length);
    let result = '';
    for (let i = 0; i < length; i++) {
      result += chars[randomBytes[i] % chars.length];
    }
    return result;
  }

  /**
   * Encrypt sensitive fields in an object
   */
  encryptFields<T extends Record<string, any>>(
    obj: T,
    fields: (keyof T)[]
  ): T {
    const encrypted = { ...obj };
    for (const field of fields) {
      if (encrypted[field]) {
        encrypted[field] = this.encrypt(String(encrypted[field])) as any;
      }
    }
    return encrypted;
  }

  /**
   * Decrypt sensitive fields in an object
   */
  decryptFields<T extends Record<string, any>>(
    obj: T,
    fields: (keyof T)[]
  ): T {
    const decrypted = { ...obj };
    for (const field of fields) {
      if (decrypted[field]) {
        try {
          decrypted[field] = this.decrypt(String(decrypted[field])) as any;
        } catch (error) {
          // Field might not be encrypted, leave as is
          console.warn(`Failed to decrypt field ${String(field)}`);
        }
      }
    }
    return decrypted;
  }
}

/**
 * Password hashing utilities
 */
export class PasswordService {
  /**
   * Hash password with salt
   */
  static async hashPassword(password: string): Promise<string> {
    const salt = crypto.randomBytes(SALT_LENGTH).toString('hex');
    const hash = await this.hashWithSalt(password, salt);
    return `${salt}:${hash}`;
  }

  /**
   * Verify password
   */
  static async verifyPassword(password: string, hashedPassword: string): Promise<boolean> {
    const [salt, hash] = hashedPassword.split(':');
    const passwordHash = await this.hashWithSalt(password, salt);
    return crypto.timingSafeEqual(Buffer.from(hash), Buffer.from(passwordHash));
  }

  /**
   * Hash password with given salt
   */
  private static async hashWithSalt(password: string, salt: string): Promise<string> {
    return new Promise((resolve, reject) => {
      crypto.pbkdf2(password, salt, 100000, 64, 'sha512', (err, derivedKey) => {
        if (err) reject(err);
        resolve(derivedKey.toString('hex'));
      });
    });
  }

  /**
   * Validate password strength
   */
  static validatePasswordStrength(password: string): {
    isValid: boolean;
    errors: string[];
  } {
    const errors: string[] = [];

    if (password.length < 8) {
      errors.push('Password must be at least 8 characters long');
    }

    if (!/[A-Z]/.test(password)) {
      errors.push('Password must contain at least one uppercase letter');
    }

    if (!/[a-z]/.test(password)) {
      errors.push('Password must contain at least one lowercase letter');
    }

    if (!/[0-9]/.test(password)) {
      errors.push('Password must contain at least one number');
    }

    if (!/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
      errors.push('Password must contain at least one special character');
    }

    return {
      isValid: errors.length === 0,
      errors,
    };
  }

  /**
   * Generate secure password
   */
  static generateSecurePassword(length: number = 16): string {
    const uppercase = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const lowercase = 'abcdefghijklmnopqrstuvwxyz';
    const numbers = '0123456789';
    const special = '!@#$%^&*(),.?":{}|<>';
    const all = uppercase + lowercase + numbers + special;

    let password = '';
    password += uppercase[Math.floor(Math.random() * uppercase.length)];
    password += lowercase[Math.floor(Math.random() * lowercase.length)];
    password += numbers[Math.floor(Math.random() * numbers.length)];
    password += special[Math.floor(Math.random() * special.length)];

    for (let i = 4; i < length; i++) {
      password += all[Math.floor(Math.random() * all.length)];
    }

    // Shuffle password
    return password
      .split('')
      .sort(() => Math.random() - 0.5)
      .join('');
  }
}

/**
 * Key rotation utilities
 */
export class KeyRotationService {
  private static rotationHistory: Array<{ key: Buffer; rotatedAt: Date }> = [];

  /**
   * Rotate encryption key
   */
  static rotateKey(newKey: string): void {
    const oldKey = process.env.ENCRYPTION_KEY;
    if (oldKey) {
      this.rotationHistory.push({
        key: Buffer.from(oldKey.slice(0, 32), 'utf-8'),
        rotatedAt: new Date(),
      });
    }

    process.env.ENCRYPTION_KEY = newKey;
    console.log('Encryption key rotated successfully');
  }

  /**
   * Get rotation history
   */
  static getRotationHistory(): Array<{ rotatedAt: Date }> {
    return this.rotationHistory.map((entry) => ({
      rotatedAt: entry.rotatedAt,
    }));
  }

  /**
   * Re-encrypt data with new key
   */
  static async reEncryptData(
    encryptedData: string,
    oldKeyIndex: number = 0
  ): Promise<string> {
    if (oldKeyIndex >= this.rotationHistory.length) {
      throw new Error('Invalid key index');
    }

    const oldKey = this.rotationHistory[oldKeyIndex].key;
    const currentKey = process.env.ENCRYPTION_KEY;

    if (!currentKey) {
      throw new Error('Current encryption key not set');
    }

    // Decrypt with old key
    const oldEncryption = new EncryptionService();
    (oldEncryption as any).encryptionKey = oldKey;
    const decrypted = oldEncryption.decrypt(encryptedData);

    // Encrypt with new key
    const newEncryption = new EncryptionService();
    return newEncryption.encrypt(decrypted);
  }
}

// Export singleton instance
export const encryptionService = new EncryptionService();

// Export utilities
export { PasswordService, KeyRotationService };
