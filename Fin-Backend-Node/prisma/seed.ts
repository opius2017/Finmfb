import { PrismaClient } from '@prisma/client';
import bcrypt from 'bcrypt';

const prisma = new PrismaClient();

async function main() {
  console.log('🌱 Starting database seeding...');

  // Clear existing data (in reverse order of dependencies)
  console.log('🧹 Cleaning existing data...');
  await prisma.approval.deleteMany();
  await prisma.approvalRequest.deleteMany();
  await prisma.bankTransaction.deleteMany();
  await prisma.bankConnection.deleteMany();
  await prisma.documentVersion.deleteMany();
  await prisma.document.deleteMany();
  await prisma.budgetActual.deleteMany();
  await prisma.budgetItem.deleteMany();
  await prisma.budget.deleteMany();
  await prisma.guarantor.deleteMany();
  await prisma.loanPayment.deleteMany();
  await prisma.loanSchedule.deleteMany();
  await prisma.loan.deleteMany();
  await prisma.loanProduct.deleteMany();
  await prisma.transaction.deleteMany();
  await prisma.account.deleteMany();
  await prisma.member.deleteMany();
  await prisma.session.deleteMany();
  await prisma.userRole.deleteMany();
  await prisma.rolePermission.deleteMany();
  await prisma.permission.deleteMany();
  await prisma.role.deleteMany();
  await prisma.user.deleteMany();
  await prisma.branch.deleteMany();
  await prisma.auditLog.deleteMany();
  await prisma.systemLog.deleteMany();

  // ============================================
  // 1. CREATE BRANCHES (Nigerian States)
  // ============================================
  console.log('🏢 Creating branches...');
  
  const branches = await Promise.all([
    prisma.branch.create({
      data: {
        name: 'Lagos Head Office',
        code: 'LOS-HQ',
        address: '15 Marina Road',
        city: 'Lagos',
        state: 'Lagos',
        country: 'Nigeria',
        status: 'ACTIVE',
      },
    }),
    prisma.branch.create({
      data: {
        name: 'Abuja Branch',
        code: 'ABJ-01',
        address: '23 Ahmadu Bello Way',
        city: 'Abuja',
        state: 'FCT',
        country: 'Nigeria',
        status: 'ACTIVE',
      },
    }),
    prisma.branch.create({
      data: {
        name: 'Port Harcourt Branch',
        code: 'PHC-01',
        address: '45 Aba Road',
        city: 'Port Harcourt',
        state: 'Rivers',
        country: 'Nigeria',
        status: 'ACTIVE',
      },
    }),
    prisma.branch.create({
      data: {
        name: 'Kano Branch',
        code: 'KAN-01',
        address: '12 Bompai Road',
        city: 'Kano',
        state: 'Kano',
        country: 'Nigeria',
        status: 'ACTIVE',
      },
    }),
    prisma.branch.create({
      data: {
        name: 'Ibadan Branch',
        code: 'IBD-01',
        address: '78 Iwo Road',
        city: 'Ibadan',
        state: 'Oyo',
        country: 'Nigeria',
        status: 'ACTIVE',
      },
    }),
  ]);

  console.log(`✅ Created ${branches.length} branches`);

  // ============================================
  // 2. CREATE ROLES & PERMISSIONS
  // ============================================
  console.log('🔐 Creating roles and permissions...');

  // Create Permissions
  const resources = ['members', 'accounts', 'transactions', 'loans', 'budgets', 'reports', 'users', 'settings', 'approvals', 'documents'];
  const actions = ['create', 'read', 'update', 'delete', 'approve'];

  const permissions = [];
  for (const resource of resources) {
    for (const action of actions) {
      const permission = await prisma.permission.create({
        data: {
          name: `${resource}:${action}`,
          description: `${action} ${resource}`,
          resource,
          action,
        },
      });
      permissions.push(permission);
    }
  }

  console.log(`✅ Created ${permissions.length} permissions`);

  // Create Roles
  const adminRole = await prisma.role.create({
    data: {
      name: 'Admin',
      description: 'System Administrator with full access',
    },
  });

  const managerRole = await prisma.role.create({
    data: {
      name: 'Branch Manager',
      description: 'Branch Manager with approval rights',
    },
  });

  const tellerRole = await prisma.role.create({
    data: {
      name: 'Teller',
      description: 'Teller for daily transactions',
    },
  });

  const loanOfficerRole = await prisma.role.create({
    data: {
      name: 'Loan Officer',
      description: 'Loan Officer for loan processing',
    },
  });

  const accountantRole = await prisma.role.create({
    data: {
      name: 'Accountant',
      description: 'Accountant for financial reporting',
    },
  });

  console.log('✅ Created 5 roles');

  // Assign Permissions to Roles
  // Admin gets all permissions
  for (const permission of permissions) {
    await prisma.rolePermission.create({
      data: {
        roleId: adminRole.id,
        permissionId: permission.id,
      },
    });
  }

  // Manager gets most permissions except user management
  const managerPermissions = permissions.filter(p => 
    !p.resource.includes('users') && !p.resource.includes('settings')
  );
  for (const permission of managerPermissions) {
    await prisma.rolePermission.create({
      data: {
        roleId: managerRole.id,
        permissionId: permission.id,
      },
    });
  }

  // Teller gets transaction and account permissions
  const tellerPermissions = permissions.filter(p => 
    ['transactions', 'accounts', 'members'].includes(p.resource) && 
    ['create', 'read'].includes(p.action)
  );
  for (const permission of tellerPermissions) {
    await prisma.rolePermission.create({
      data: {
        roleId: tellerRole.id,
        permissionId: permission.id,
      },
    });
  }

  // Loan Officer gets loan permissions
  const loanOfficerPermissions = permissions.filter(p => 
    ['loans', 'members', 'documents'].includes(p.resource)
  );
  for (const permission of loanOfficerPermissions) {
    await prisma.rolePermission.create({
      data: {
        roleId: loanOfficerRole.id,
        permissionId: permission.id,
      },
    });
  }

  // Accountant gets budget and report permissions
  const accountantPermissions = permissions.filter(p => 
    ['budgets', 'reports', 'transactions'].includes(p.resource)
  );
  for (const permission of accountantPermissions) {
    await prisma.rolePermission.create({
      data: {
        roleId: accountantRole.id,
        permissionId: permission.id,
      },
    });
  }

  console.log('✅ Assigned permissions to roles');

  // ============================================
  // 3. CREATE USERS
  // ============================================
  console.log('👥 Creating users...');

  const hashedPassword = await bcrypt.hash('Password123!', 12);

  const adminUser = await prisma.user.create({
    data: {
      email: 'admin@finmfb.ng',
      passwordHash: hashedPassword,
      firstName: 'Chukwuemeka',
      lastName: 'Okonkwo',
      status: 'ACTIVE',
    },
  });

  await prisma.userRole.create({
    data: {
      userId: adminUser.id,
      roleId: adminRole.id,
    },
  });

  const managerUser = await prisma.user.create({
    data: {
      email: 'manager@finmfb.ng',
      passwordHash: hashedPassword,
      firstName: 'Aisha',
      lastName: 'Bello',
      status: 'ACTIVE',
    },
  });

  await prisma.userRole.create({
    data: {
      userId: managerUser.id,
      roleId: managerRole.id,
    },
  });

  const tellerUser = await prisma.user.create({
    data: {
      email: 'teller@finmfb.ng',
      passwordHash: hashedPassword,
      firstName: 'Ngozi',
      lastName: 'Eze',
      status: 'ACTIVE',
    },
  });

  await prisma.userRole.create({
    data: {
      userId: tellerUser.id,
      roleId: tellerRole.id,
    },
  });

  const loanOfficerUser = await prisma.user.create({
    data: {
      email: 'loanofficer@finmfb.ng',
      passwordHash: hashedPassword,
      firstName: 'Oluwaseun',
      lastName: 'Adeyemi',
      status: 'ACTIVE',
    },
  });

  await prisma.userRole.create({
    data: {
      userId: loanOfficerUser.id,
      roleId: loanOfficerRole.id,
    },
  });

  const accountantUser = await prisma.user.create({
    data: {
      email: 'accountant@finmfb.ng',
      passwordHash: hashedPassword,
      firstName: 'Ibrahim',
      lastName: 'Musa',
      status: 'ACTIVE',
    },
  });

  await prisma.userRole.create({
    data: {
      userId: accountantUser.id,
      roleId: accountantRole.id,
    },
  });

  console.log('✅ Created 5 users');

  // ============================================
  // 4. CREATE MEMBERS (Nigerian Names)
  // ============================================
  console.log('👨‍👩‍👧‍👦 Creating members...');

  const nigerianMembers = [
    { firstName: 'Adebayo', lastName: 'Ogunleye', email: 'adebayo.ogunleye@email.com', phone: '08012345678', city: 'Lagos', state: 'Lagos' },
    { firstName: 'Chidinma', lastName: 'Nwosu', email: 'chidinma.nwosu@email.com', phone: '08023456789', city: 'Enugu', state: 'Enugu' },
    { firstName: 'Fatima', lastName: 'Abdullahi', email: 'fatima.abdullahi@email.com', phone: '08034567890', city: 'Kano', state: 'Kano' },
    { firstName: 'Emeka', lastName: 'Okafor', email: 'emeka.okafor@email.com', phone: '08045678901', city: 'Aba', state: 'Abia' },
    { firstName: 'Blessing', lastName: 'Okoro', email: 'blessing.okoro@email.com', phone: '08056789012', city: 'Port Harcourt', state: 'Rivers' },
    { firstName: 'Yusuf', lastName: 'Mohammed', email: 'yusuf.mohammed@email.com', phone: '08067890123', city: 'Abuja', state: 'FCT' },
    { firstName: 'Nneka', lastName: 'Chukwu', email: 'nneka.chukwu@email.com', phone: '08078901234', city: 'Onitsha', state: 'Anambra' },
    { firstName: 'Tunde', lastName: 'Bakare', email: 'tunde.bakare@email.com', phone: '08089012345', city: 'Ibadan', state: 'Oyo' },
    { firstName: 'Amina', lastName: 'Suleiman', email: 'amina.suleiman@email.com', phone: '08090123456', city: 'Kaduna', state: 'Kaduna' },
    { firstName: 'Chinedu', lastName: 'Ibe', email: 'chinedu.ibe@email.com', phone: '08101234567', city: 'Owerri', state: 'Imo' },
    { firstName: 'Folake', lastName: 'Williams', email: 'folake.williams@email.com', phone: '08112345678', city: 'Lagos', state: 'Lagos' },
    { firstName: 'Musa', lastName: 'Garba', email: 'musa.garba@email.com', phone: '08123456789', city: 'Jos', state: 'Plateau' },
    { firstName: 'Chiamaka', lastName: 'Obi', email: 'chiamaka.obi@email.com', phone: '08134567890', city: 'Awka', state: 'Anambra' },
    { firstName: 'Segun', lastName: 'Ajayi', email: 'segun.ajayi@email.com', phone: '08145678901', city: 'Abeokuta', state: 'Ogun' },
    { firstName: 'Zainab', lastName: 'Ibrahim', email: 'zainab.ibrahim@email.com', phone: '08156789012', city: 'Sokoto', state: 'Sokoto' },
    { firstName: 'Obinna', lastName: 'Nnamdi', email: 'obinna.nnamdi@email.com', phone: '08167890123', city: 'Umuahia', state: 'Abia' },
    { firstName: 'Funke', lastName: 'Adeyemi', email: 'funke.adeyemi@email.com', phone: '08178901234', city: 'Akure', state: 'Ondo' },
    { firstName: 'Aliyu', lastName: 'Bello', email: 'aliyu.bello@email.com', phone: '08189012345', city: 'Minna', state: 'Niger' },
    { firstName: 'Ifeoma', lastName: 'Eze', email: 'ifeoma.eze@email.com', phone: '08190123456', city: 'Nsukka', state: 'Enugu' },
    { firstName: 'Kunle', lastName: 'Oladipo', email: 'kunle.oladipo@email.com', phone: '08201234567', city: 'Ile-Ife', state: 'Osun' },
  ];

  const members = [];
  for (let i = 0; i < nigerianMembers.length; i++) {
    const memberData = nigerianMembers[i];
    const member = await prisma.member.create({
      data: {
        memberNumber: `MEM${String(i + 1).padStart(6, '0')}`,
        firstName: memberData.firstName,
        lastName: memberData.lastName,
        email: memberData.email,
        phone: memberData.phone,
        dateOfBirth: new Date(1980 + Math.floor(Math.random() * 30), Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1),
        address: `${Math.floor(Math.random() * 100) + 1} ${['Allen Avenue', 'Broad Street', 'Ahmadu Bello Way', 'Aba Road', 'Ring Road'][Math.floor(Math.random() * 5)]}`,
        city: memberData.city,
        state: memberData.state,
        country: 'Nigeria',
        status: 'ACTIVE',
        branchId: branches[i % branches.length].id,
        joinDate: new Date(2020 + Math.floor(Math.random() * 4), Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1),
      },
    });
    members.push(member);
  }

  console.log(`✅ Created ${members.length} members`);

  // ============================================
  // 5. CREATE ACCOUNTS
  // ============================================
  console.log('💰 Creating accounts...');

  const accounts = [];
  for (const member of members) {
    // Create Savings Account
    const savingsAccount = await prisma.account.create({
      data: {
        accountNumber: `SAV${String(accounts.length + 1).padStart(10, '0')}`,
        memberId: member.id,
        type: 'SAVINGS',
        balance: Math.floor(Math.random() * 500000) + 10000,
        status: 'ACTIVE',
        branchId: member.branchId,
      },
    });
    accounts.push(savingsAccount);

    // Some members also have shares account
    if (Math.random() > 0.5) {
      const sharesAccount = await prisma.account.create({
        data: {
          accountNumber: `SHR${String(accounts.length + 1).padStart(10, '0')}`,
          memberId: member.id,
          type: 'SHARES',
          balance: Math.floor(Math.random() * 100000) + 5000,
          status: 'ACTIVE',
          branchId: member.branchId,
        },
      });
      accounts.push(sharesAccount);
    }
  }

  console.log(`✅ Created ${accounts.length} accounts`);

  // ============================================
  // 6. CREATE TRANSACTIONS
  // ============================================
  console.log('💸 Creating transactions...');

  const transactionTypes = ['DEBIT', 'CREDIT'];
  const descriptions = ['Deposit', 'Withdrawal', 'Transfer', 'Interest Payment', 'Loan Repayment', 'Dividend Payment'];

  let transactionCount = 0;
  for (const account of accounts.slice(0, 10)) {
    // Create 5-10 transactions per account
    const numTransactions = Math.floor(Math.random() * 6) + 5;
    
    for (let i = 0; i < numTransactions; i++) {
      const type = transactionTypes[Math.floor(Math.random() * transactionTypes.length)];
      const amount = Math.floor(Math.random() * 50000) + 1000;
      
      await prisma.transaction.create({
        data: {
          accountId: account.id,
          type,
          amount,
          description: descriptions[Math.floor(Math.random() * descriptions.length)],
          reference: `TXN${Date.now()}${String(transactionCount++).padStart(6, '0')}`,
          status: 'COMPLETED',
          createdBy: tellerUser.id,
          createdAt: new Date(2024, Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1),
        },
      });
    }
  }

  console.log(`✅ Created ${transactionCount} transactions`);

  // ============================================
  // 7. CREATE LOAN PRODUCTS
  // ============================================
  console.log('🏦 Creating loan products...');

  const loanProducts = await Promise.all([
    prisma.loanProduct.create({
      data: {
        name: 'Personal Loan',
        description: 'Short-term personal loan for individuals',
        interestRate: 0.15, // 15% annual
        minAmount: 50000,
        maxAmount: 500000,
        minTermMonths: 3,
        maxTermMonths: 12,
        calculationMethod: 'reducing_balance',
        penaltyRate: 0.02,
        isActive: true,
      },
    }),
    prisma.loanProduct.create({
      data: {
        name: 'Business Loan',
        description: 'Medium-term loan for small businesses',
        interestRate: 0.18, // 18% annual
        minAmount: 100000,
        maxAmount: 2000000,
        minTermMonths: 6,
        maxTermMonths: 24,
        calculationMethod: 'reducing_balance',
        penaltyRate: 0.02,
        isActive: true,
      },
    }),
    prisma.loanProduct.create({
      data: {
        name: 'Emergency Loan',
        description: 'Quick emergency loan with fast approval',
        interestRate: 0.20, // 20% annual
        minAmount: 20000,
        maxAmount: 200000,
        minTermMonths: 1,
        maxTermMonths: 6,
        calculationMethod: 'flat_rate',
        penaltyRate: 0.03,
        isActive: true,
      },
    }),
    prisma.loanProduct.create({
      data: {
        name: 'Asset Finance',
        description: 'Loan for purchasing business assets',
        interestRate: 0.16, // 16% annual
        minAmount: 500000,
        maxAmount: 5000000,
        minTermMonths: 12,
        maxTermMonths: 36,
        calculationMethod: 'reducing_balance',
        penaltyRate: 0.02,
        isActive: true,
      },
    }),
    prisma.loanProduct.create({
      data: {
        name: 'Salary Advance',
        description: 'Short-term salary advance for salaried workers',
        interestRate: 0.12, // 12% annual
        minAmount: 30000,
        maxAmount: 300000,
        minTermMonths: 1,
        maxTermMonths: 3,
        calculationMethod: 'flat_rate',
        penaltyRate: 0.01,
        isActive: true,
      },
    }),
  ]);

  console.log(`✅ Created ${loanProducts.length} loan products`);

  // ============================================
  // 8. CREATE LOANS
  // ============================================
  console.log('💳 Creating loans...');

  const loanStatuses = ['PENDING', 'APPROVED', 'DISBURSED', 'ACTIVE', 'CLOSED'];
  const loanPurposes = [
    'Business Expansion',
    'Working Capital',
    'Equipment Purchase',
    'Stock Purchase',
    'Emergency Medical',
    'Education',
    'Home Improvement',
    'Debt Consolidation',
  ];

  const loans = [];
  for (let i = 0; i < 15; i++) {
    const member = members[i];
    const loanProduct = loanProducts[Math.floor(Math.random() * loanProducts.length)];
    const requestedAmount = Math.floor(Math.random() * (Number(loanProduct.maxAmount) - Number(loanProduct.minAmount))) + Number(loanProduct.minAmount);
    const status = loanStatuses[Math.floor(Math.random() * loanStatuses.length)];
    
    const loan = await prisma.loan.create({
      data: {
        memberId: member.id,
        loanProductId: loanProduct.id,
        requestedAmount,
        approvedAmount: ['APPROVED', 'DISBURSED', 'ACTIVE', 'CLOSED'].includes(status) ? requestedAmount : 0,
        disbursedAmount: ['DISBURSED', 'ACTIVE', 'CLOSED'].includes(status) ? requestedAmount : 0,
        outstandingBalance: status === 'ACTIVE' ? requestedAmount * 0.7 : (status === 'CLOSED' ? 0 : requestedAmount),
        interestRate: loanProduct.interestRate,
        termMonths: Math.floor(Math.random() * (loanProduct.maxTermMonths - loanProduct.minTermMonths)) + loanProduct.minTermMonths,
        purpose: loanPurposes[Math.floor(Math.random() * loanPurposes.length)],
        collateralDescription: 'Personal Guarantee',
        status,
        applicationDate: new Date(2024, Math.floor(Math.random() * 6), Math.floor(Math.random() * 28) + 1),
        approvalDate: ['APPROVED', 'DISBURSED', 'ACTIVE', 'CLOSED'].includes(status) ? new Date(2024, Math.floor(Math.random() * 6) + 1, Math.floor(Math.random() * 28) + 1) : null,
        disbursementDate: ['DISBURSED', 'ACTIVE', 'CLOSED'].includes(status) ? new Date(2024, Math.floor(Math.random() * 6) + 2, Math.floor(Math.random() * 28) + 1) : null,
        closedDate: status === 'CLOSED' ? new Date(2024, 10, Math.floor(Math.random() * 28) + 1) : null,
        createdBy: loanOfficerUser.id,
      },
    });
    loans.push(loan);
  }

  console.log(`✅ Created ${loans.length} loans`);

  // ============================================
  // 9. CREATE BUDGETS
  // ============================================
  console.log('📊 Creating budgets...');

  const budget2024 = await prisma.budget.create({
    data: {
      name: '2024 Annual Budget',
      description: 'Annual operational budget for 2024',
      startDate: new Date(2024, 0, 1),
      endDate: new Date(2024, 11, 31),
      fiscalYear: 2024,
      totalAmount: 50000000,
      status: 'APPROVED',
      branchId: branches[0].id,
      createdBy: accountantUser.id,
    },
  });

  const budgetCategories = [
    { name: 'Staff Salaries', category: 'Personnel', amount: 20000000 },
    { name: 'Office Rent', category: 'Operations', amount: 5000000 },
    { name: 'Marketing & Advertising', category: 'Marketing', amount: 3000000 },
    { name: 'IT Infrastructure', category: 'Technology', amount: 4000000 },
    { name: 'Training & Development', category: 'Personnel', amount: 2000000 },
    { name: 'Utilities', category: 'Operations', amount: 1500000 },
    { name: 'Office Supplies', category: 'Operations', amount: 1000000 },
    { name: 'Professional Services', category: 'Operations', amount: 2500000 },
    { name: 'Insurance', category: 'Operations', amount: 1500000 },
    { name: 'Contingency', category: 'Reserves', amount: 9500000 },
  ];

  for (const item of budgetCategories) {
    await prisma.budgetItem.create({
      data: {
        budgetId: budget2024.id,
        name: item.name,
        category: item.category,
        amount: item.amount,
        description: `Budget allocation for ${item.name}`,
      },
    });
  }

  console.log('✅ Created budget with items');

  // ============================================
  // 10. CREATE BANK CONNECTIONS
  // ============================================
  console.log('🏦 Creating bank connections...');

  const nigerianBanks = [
    { name: 'Access Bank', code: '044' },
    { name: 'GTBank', code: '058' },
    { name: 'First Bank', code: '011' },
    { name: 'UBA', code: '033' },
    { name: 'Zenith Bank', code: '057' },
  ];

  for (let i = 0; i < 3; i++) {
    const bank = nigerianBanks[i];
    await prisma.bankConnection.create({
      data: {
        bankName: bank.name,
        accountNumber: `${Math.floor(Math.random() * 9000000000) + 1000000000}`,
        accountName: 'FinMFB Operations Account',
        bankCode: bank.code,
        branchId: branches[i].id,
        status: 'ACTIVE',
        createdBy: adminUser.id,
      },
    });
  }

  console.log('✅ Created 3 bank connections');

  // ============================================
  // SUMMARY
  // ============================================
  console.log('\n🎉 Database seeding completed successfully!');
  console.log('\n📋 Summary:');
  console.log(`   - Branches: ${branches.length}`);
  console.log(`   - Users: 5`);
  console.log(`   - Roles: 5`);
  console.log(`   - Permissions: ${permissions.length}`);
  console.log(`   - Members: ${members.length}`);
  console.log(`   - Accounts: ${accounts.length}`);
  console.log(`   - Transactions: ${transactionCount}`);
  console.log(`   - Loan Products: ${loanProducts.length}`);
  console.log(`   - Loans: ${loans.length}`);
  console.log(`   - Budgets: 1`);
  console.log(`   - Bank Connections: 3`);
  
  console.log('\n🔑 Default Login Credentials:');
  console.log('   Admin:         admin@finmfb.ng / Password123!');
  console.log('   Manager:       manager@finmfb.ng / Password123!');
  console.log('   Teller:        teller@finmfb.ng / Password123!');
  console.log('   Loan Officer:  loanofficer@finmfb.ng / Password123!');
  console.log('   Accountant:    accountant@finmfb.ng / Password123!');
}

main()
  .catch((e) => {
    console.error('❌ Error seeding database:', e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
