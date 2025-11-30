require('dotenv').config();

console.log('NODE_ENV:', process.env.NODE_ENV);
console.log('PORT:', process.env.PORT);
console.log('DATABASE_URL:', process.env.DATABASE_URL ? 'SET' : 'NOT SET');
console.log('ENCRYPTION_KEY:', process.env.ENCRYPTION_KEY ? `SET (length: ${process.env.ENCRYPTION_KEY.length})` : 'NOT SET');
console.log('JWT_SECRET:', process.env.JWT_SECRET ? 'SET' : 'NOT SET');
