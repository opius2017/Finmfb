import nodemailer from 'nodemailer';
import { createBadRequestError } from '@middleware/errorHandler';
import Handlebars from 'handlebars';
import fs from 'fs';
import path from 'path';

export interface EmailOptions {
  to: string | string[];
  subject: string;
  template?: string;
  data?: Record<string, any>;
  html?: string;
  text?: string;
  attachments?: Array<{
    filename: string;
    content?: Buffer;
    path?: string;
  }>;
}

export interface BulkEmailOptions {
  recipients: string[];
  subject: string;
  template: string;
  data: Record<string, any>;
}

export class EmailService {
  private transporter: nodemailer.Transporter;
  private templates: Map<string, HandlebarsTemplateDelegate> = new Map();

  constructor() {
    this.initializeTransporter();
    this.loadTemplates();
  }

  /**
   * Initialize email transporter
   */
  private initializeTransporter() {
    const config = {
      host: process.env.SMTP_HOST || 'smtp.gmail.com',
      port: parseInt(process.env.SMTP_PORT || '587'),
      secure: process.env.SMTP_SECURE === 'true',
      auth: {
        user: process.env.SMTP_USER,
        pass: process.env.SMTP_PASSWORD,
      },
    };

    this.transporter = nodemailer.createTransporter(config);

    // Verify connection
    this.transporter.verify((error) => {
      if (error) {
        console.error('Email service connection error:', error);
      } else {
        console.log('Email service ready');
      }
    });
  }

  /**
   * Load email templates
   */
  private loadTemplates() {
    const templatesDir = path.join(__dirname, '../templates/emails');

    // Create templates directory if it doesn't exist
    if (!fs.existsSync(templatesDir)) {
      fs.mkdirSync(templatesDir, { recursive: true });
    }

    // Load all .hbs files from templates directory
    try {
      const files = fs.readdirSync(templatesDir);
      files.forEach((file) => {
        if (file.endsWith('.hbs')) {
          const templateName = file.replace('.hbs', '');
          const templatePath = path.join(templatesDir, file);
          const templateContent = fs.readFileSync(templatePath, 'utf-8');
          this.templates.set(templateName, Handlebars.compile(templateContent));
        }
      });
    } catch (error) {
      console.warn('No email templates found, using default templates');
    }

    // Register default templates
    this.registerDefaultTemplates();
  }

  /**
   * Register default email templates
   */
  private registerDefaultTemplates() {
    // Welcome email
    this.templates.set(
      'welcome',
      Handlebars.compile(`
        <h1>Welcome to FinMFB, {{name}}!</h1>
        <p>Thank you for joining us. Your account has been created successfully.</p>
        <p>You can now access all our services.</p>
        <p>Best regards,<br>The FinMFB Team</p>
      `)
    );

    // Password reset
    this.templates.set(
      'password-reset',
      Handlebars.compile(`
        <h1>Password Reset Request</h1>
        <p>Hi {{name}},</p>
        <p>You requested to reset your password. Click the link below to reset it:</p>
        <p><a href="{{resetUrl}}">Reset Password</a></p>
        <p>This link will expire in {{expiryHours}} hours.</p>
        <p>If you didn't request this, please ignore this email.</p>
        <p>Best regards,<br>The FinMFB Team</p>
      `)
    );

    // Loan approval
    this.templates.set(
      'loan-approved',
      Handlebars.compile(`
        <h1>Loan Application Approved</h1>
        <p>Dear {{memberName}},</p>
        <p>Congratulations! Your loan application has been approved.</p>
        <p><strong>Loan Details:</strong></p>
        <ul>
          <li>Amount: ₦{{amount}}</li>
          <li>Interest Rate: {{interestRate}}%</li>
          <li>Term: {{termMonths}} months</li>
        </ul>
        <p>The funds will be disbursed to your account shortly.</p>
        <p>Best regards,<br>The FinMFB Team</p>
      `)
    );

    // Loan rejection
    this.templates.set(
      'loan-rejected',
      Handlebars.compile(`
        <h1>Loan Application Update</h1>
        <p>Dear {{memberName}},</p>
        <p>We regret to inform you that your loan application has not been approved at this time.</p>
        <p><strong>Reason:</strong> {{reason}}</p>
        <p>You may reapply after addressing the concerns mentioned above.</p>
        <p>Best regards,<br>The FinMFB Team</p>
      `)
    );

    // Payment reminder
    this.templates.set(
      'payment-reminder',
      Handlebars.compile(`
        <h1>Payment Reminder</h1>
        <p>Dear {{memberName}},</p>
        <p>This is a friendly reminder that your loan payment is due soon.</p>
        <p><strong>Payment Details:</strong></p>
        <ul>
          <li>Amount Due: ₦{{amount}}</li>
          <li>Due Date: {{dueDate}}</li>
          <li>Loan Number: {{loanNumber}}</li>
        </ul>
        <p>Please ensure payment is made on or before the due date to avoid penalties.</p>
        <p>Best regards,<br>The FinMFB Team</p>
      `)
    );

    // Transaction notification
    this.templates.set(
      'transaction-notification',
      Handlebars.compile(`
        <h1>Transaction Notification</h1>
        <p>Dear {{memberName}},</p>
        <p>A transaction has been processed on your account.</p>
        <p><strong>Transaction Details:</strong></p>
        <ul>
          <li>Type: {{type}}</li>
          <li>Amount: ₦{{amount}}</li>
          <li>Date: {{date}}</li>
          <li>Reference: {{reference}}</li>
          <li>New Balance: ₦{{balance}}</li>
        </ul>
        <p>If you did not authorize this transaction, please contact us immediately.</p>
        <p>Best regards,<br>The FinMFB Team</p>
      `)
    );
  }

  /**
   * Send email
   */
  async sendEmail(options: EmailOptions): Promise<void> {
    try {
      let html = options.html;
      let text = options.text;

      // Use template if specified
      if (options.template) {
        const template = this.templates.get(options.template);
        if (!template) {
          throw createBadRequestError(`Email template '${options.template}' not found`);
        }
        html = template(options.data || {});
      }

      const mailOptions = {
        from: process.env.SMTP_FROM || 'noreply@finmfb.ng',
        to: Array.isArray(options.to) ? options.to.join(', ') : options.to,
        subject: options.subject,
        html,
        text,
        attachments: options.attachments,
      };

      await this.transporter.sendMail(mailOptions);
      console.log(`Email sent to ${mailOptions.to}`);
    } catch (error) {
      console.error('Failed to send email:', error);
      throw error;
    }
  }

  /**
   * Send bulk emails
   */
  async sendBulkEmails(options: BulkEmailOptions): Promise<void> {
    const template = this.templates.get(options.template);
    if (!template) {
      throw createBadRequestError(`Email template '${options.template}' not found`);
    }

    const html = template(options.data);

    const promises = options.recipients.map((recipient) =>
      this.sendEmail({
        to: recipient,
        subject: options.subject,
        html,
      })
    );

    await Promise.allSettled(promises);
  }

  /**
   * Send welcome email
   */
  async sendWelcomeEmail(to: string, name: string): Promise<void> {
    await this.sendEmail({
      to,
      subject: 'Welcome to FinMFB',
      template: 'welcome',
      data: { name },
    });
  }

  /**
   * Send password reset email
   */
  async sendPasswordResetEmail(
    to: string,
    name: string,
    resetUrl: string,
    expiryHours: number = 1
  ): Promise<void> {
    await this.sendEmail({
      to,
      subject: 'Password Reset Request',
      template: 'password-reset',
      data: { name, resetUrl, expiryHours },
    });
  }

  /**
   * Send loan approval email
   */
  async sendLoanApprovalEmail(
    to: string,
    memberName: string,
    loanDetails: {
      amount: number;
      interestRate: number;
      termMonths: number;
    }
  ): Promise<void> {
    await this.sendEmail({
      to,
      subject: 'Loan Application Approved',
      template: 'loan-approved',
      data: {
        memberName,
        ...loanDetails,
      },
    });
  }

  /**
   * Send loan rejection email
   */
  async sendLoanRejectionEmail(
    to: string,
    memberName: string,
    reason: string
  ): Promise<void> {
    await this.sendEmail({
      to,
      subject: 'Loan Application Update',
      template: 'loan-rejected',
      data: {
        memberName,
        reason,
      },
    });
  }

  /**
   * Send payment reminder
   */
  async sendPaymentReminder(
    to: string,
    memberName: string,
    paymentDetails: {
      amount: number;
      dueDate: string;
      loanNumber: string;
    }
  ): Promise<void> {
    await this.sendEmail({
      to,
      subject: 'Payment Reminder',
      template: 'payment-reminder',
      data: {
        memberName,
        ...paymentDetails,
      },
    });
  }

  /**
   * Send transaction notification
   */
  async sendTransactionNotification(
    to: string,
    memberName: string,
    transactionDetails: {
      type: string;
      amount: number;
      date: string;
      reference: string;
      balance: number;
    }
  ): Promise<void> {
    await this.sendEmail({
      to,
      subject: 'Transaction Notification',
      template: 'transaction-notification',
      data: {
        memberName,
        ...transactionDetails,
      },
    });
  }

  /**
   * Verify email service connection
   */
  async verifyConnection(): Promise<boolean> {
    try {
      await this.transporter.verify();
      return true;
    } catch (error) {
      console.error('Email service verification failed:', error);
      return false;
    }
  }
}

export default EmailService;
