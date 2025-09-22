// @ts-nocheck
import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Loader2 } from 'lucide-react';
import { useGetCustomerQuery } from '../../services/customersApi';
import { useCreateLoanMutation } from '../../services/loansApi';
import { toast } from '../../services/toast';

// Loan application schema
const loanApplicationSchema = z.object({
  amount: z.number().min(1000).max(10000000),
  term: z.number().min(1).max(60),
  purpose: z.string().min(10).max(500),
  collateralType: z.enum(['real_estate', 'vehicle', 'fixed_deposit', 'shares', 'none']),
  collateralValue: z.number().optional(),
  collateralDescription: z.string().optional(),
  monthlyIncome: z.number().min(0),
  employmentType: z.enum(['employed', 'self_employed', 'business_owner', 'retired']),
  employerName: z.string().optional(),
  yearsEmployed: z.number().optional(),
  businessType: z.string().optional(),
  yearsInBusiness: z.number().optional(),
  otherLoans: z.array(z.object({
    lender: z.string(),
    amount: z.number(),
    monthlyPayment: z.number(),
    remainingBalance: z.number(),
  })).optional(),
  references: z.array(z.object({
    name: z.string(),
    relationship: z.string(),
    phoneNumber: z.string(),
    email: z.string().email().optional(),
  })).min(2),
});

type LoanApplicationData = z.infer<typeof loanApplicationSchema>;

const employmentTypes = [
  { value: 'employed', label: 'Employed' },
  { value: 'self_employed', label: 'Self Employed' },
  { value: 'business_owner', label: 'Business Owner' },
  { value: 'retired', label: 'Retired' },
];

const collateralTypes = [
  { value: 'real_estate', label: 'Real Estate' },
  { value: 'vehicle', label: 'Vehicle' },
  { value: 'fixed_deposit', label: 'Fixed Deposit' },
  { value: 'shares', label: 'Shares/Stocks' },
  { value: 'none', label: 'None' },
];

const LoanApplication: React.FC = () => {
  const { customerId } = useParams<{ customerId: string }>();
  const { data: customer } = useGetCustomerQuery(customerId!);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
    setValue,
  } = useForm<LoanApplicationData>({
    resolver: zodResolver(loanApplicationSchema),
    defaultValues: {
      references: [{}, {}], // Initialize with two empty reference objects
    },
  });

  const watchEmploymentType = watch('employmentType');
  const watchCollateralType = watch('collateralType');
  const watchAmount = watch('amount');
  const watchTerm = watch('term');

  const calculateMonthlyPayment = (amount: number, term: number) => {
    const interestRate = 0.15; // 15% annual interest rate
    const monthlyRate = interestRate / 12;
    const denominator = 1 - Math.pow(1 + monthlyRate, -term);
    return (amount * monthlyRate) / denominator;
  };

  const [createLoan] = useCreateLoanMutation();
  const navigate = useNavigate();

  const onSubmit = async (data: LoanApplicationData) => {
    setIsSubmitting(true);
    try {
      const response = await createLoan({
        ...data,
        customerId: customerId!,
      }).unwrap();
      toast({
        title: 'Application Submitted',
        description: 'Your loan application has been submitted successfully.',
        status: 'success',
        duration: 5000,
        isClosable: true,
      });
      navigate('/loans/management');
    } catch (error) {
      console.error('Error submitting loan application:', error);
      toast({
        title: 'Submission Failed',
        description: 'Failed to submit loan application. Please try again.',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="p-6 space-y-6">
      <div>
        <h2 className="text-lg font-medium text-gray-900">Loan Application</h2>
        <p className="mt-1 text-sm text-gray-500">
          Apply for a loan by filling out the form below
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
        {/* Loan Details */}
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Loan Details</h3>
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
            <div>
              <label htmlFor="amount" className="block text-sm font-medium text-gray-700">
                Loan Amount
              </label>
              <div className="mt-1 relative rounded-md shadow-sm">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <span className="text-gray-500 sm:text-sm">₦</span>
                </div>
                <input
                  type="number"
                  {...register('amount', { valueAsNumber: true })}
                  className="focus:ring-emerald-500 focus:border-emerald-500 block w-full pl-7 pr-12 sm:text-sm border-gray-300 rounded-md"
                  placeholder="0.00"
                />
              </div>
              {errors.amount && (
                <p className="mt-2 text-sm text-red-600">{errors.amount.message}</p>
              )}
            </div>

            <div>
              <label htmlFor="term" className="block text-sm font-medium text-gray-700">
                Loan Term (months)
              </label>
              <input
                type="number"
                {...register('term', { valueAsNumber: true })}
                className="mt-1 focus:ring-emerald-500 focus:border-emerald-500 block w-full sm:text-sm border-gray-300 rounded-md"
              />
              {errors.term && (
                <p className="mt-2 text-sm text-red-600">{errors.term.message}</p>
              )}
            </div>

            {watchAmount && watchTerm && (
              <div className="sm:col-span-2">
                <div className="rounded-md bg-emerald-50 p-4">
                  <div className="flex">
                    <div className="ml-3">
                      <h3 className="text-sm font-medium text-emerald-800">
                        Estimated Monthly Payment
                      </h3>
                      <div className="mt-2 text-sm text-emerald-700">
                        <p>
                          ₦
                          {calculateMonthlyPayment(watchAmount, watchTerm).toLocaleString(
                            undefined,
                            { maximumFractionDigits: 2 }
                          )}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            )}

            <div className="sm:col-span-2">
              <label htmlFor="purpose" className="block text-sm font-medium text-gray-700">
                Loan Purpose
              </label>
              <textarea
                {...register('purpose')}
                rows={3}
                className="mt-1 focus:ring-emerald-500 focus:border-emerald-500 block w-full sm:text-sm border-gray-300 rounded-md"
              />
              {errors.purpose && (
                <p className="mt-2 text-sm text-red-600">{errors.purpose.message}</p>
              )}
            </div>
          </div>
        </div>

        {/* Employment Information */}
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Employment Information</h3>
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
            <div>
              <label htmlFor="employmentType" className="block text-sm font-medium text-gray-700">
                Employment Type
              </label>
              <select
                {...register('employmentType')}
                className="mt-1 block w-full py-2 px-3 border border-gray-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-emerald-500 focus:border-emerald-500 sm:text-sm"
              >
                <option value="">Select type</option>
                {employmentTypes.map((type) => (
                  <option key={type.value} value={type.value}>
                    {type.label}
                  </option>
                ))}
              </select>
              {errors.employmentType && (
                <p className="mt-2 text-sm text-red-600">{errors.employmentType.message}</p>
              )}
            </div>

            <div>
              <label htmlFor="monthlyIncome" className="block text-sm font-medium text-gray-700">
                Monthly Income
              </label>
              <div className="mt-1 relative rounded-md shadow-sm">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <span className="text-gray-500 sm:text-sm">₦</span>
                </div>
                <input
                  type="number"
                  {...register('monthlyIncome', { valueAsNumber: true })}
                  className="focus:ring-emerald-500 focus:border-emerald-500 block w-full pl-7 pr-12 sm:text-sm border-gray-300 rounded-md"
                />
              </div>
              {errors.monthlyIncome && (
                <p className="mt-2 text-sm text-red-600">{errors.monthlyIncome.message}</p>
              )}
            </div>

            {watchEmploymentType === 'employed' && (
              <>
                <div>
                  <label htmlFor="employerName" className="block text-sm font-medium text-gray-700">
                    Employer Name
                  </label>
                  <input
                    type="text"
                    {...register('employerName')}
                    className="mt-1 focus:ring-emerald-500 focus:border-emerald-500 block w-full sm:text-sm border-gray-300 rounded-md"
                  />
                </div>
                <div>
                  <label htmlFor="yearsEmployed" className="block text-sm font-medium text-gray-700">
                    Years Employed
                  </label>
                  <input
                    type="number"
                    {...register('yearsEmployed', { valueAsNumber: true })}
                    className="mt-1 focus:ring-emerald-500 focus:border-emerald-500 block w-full sm:text-sm border-gray-300 rounded-md"
                  />
                </div>
              </>
            )}

            {(watchEmploymentType === 'self_employed' || watchEmploymentType === 'business_owner') && (
              <>
                <div>
                  <label htmlFor="businessType" className="block text-sm font-medium text-gray-700">
                    Business Type
                  </label>
                  <input
                    type="text"
                    {...register('businessType')}
                    className="mt-1 focus:ring-emerald-500 focus:border-emerald-500 block w-full sm:text-sm border-gray-300 rounded-md"
                  />
                </div>
                <div>
                  <label htmlFor="yearsInBusiness" className="block text-sm font-medium text-gray-700">
                    Years in Business
                  </label>
                  <input
                    type="number"
                    {...register('yearsInBusiness', { valueAsNumber: true })}
                    className="mt-1 focus:ring-emerald-500 focus:border-emerald-500 block w-full sm:text-sm border-gray-300 rounded-md"
                  />
                </div>
              </>
            )}
          </div>
        </div>

        {/* Collateral Information */}
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Collateral Information</h3>
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
            <div className="sm:col-span-2">
              <label htmlFor="collateralType" className="block text-sm font-medium text-gray-700">
                Collateral Type
              </label>
              <select
                {...register('collateralType')}
                className="mt-1 block w-full py-2 px-3 border border-gray-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-emerald-500 focus:border-emerald-500 sm:text-sm"
              >
                <option value="">Select type</option>
                {collateralTypes.map((type) => (
                  <option key={type.value} value={type.value}>
                    {type.label}
                  </option>
                ))}
              </select>
            </div>

            {watchCollateralType !== 'none' && (
              <>
                <div>
                  <label htmlFor="collateralValue" className="block text-sm font-medium text-gray-700">
                    Estimated Value
                  </label>
                  <div className="mt-1 relative rounded-md shadow-sm">
                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                      <span className="text-gray-500 sm:text-sm">₦</span>
                    </div>
                    <input
                      type="number"
                      {...register('collateralValue', { valueAsNumber: true })}
                      className="focus:ring-emerald-500 focus:border-emerald-500 block w-full pl-7 pr-12 sm:text-sm border-gray-300 rounded-md"
                    />
                  </div>
                </div>
                <div className="sm:col-span-2">
                  <label
                    htmlFor="collateralDescription"
                    className="block text-sm font-medium text-gray-700"
                  >
                    Description
                  </label>
                  <textarea
                    {...register('collateralDescription')}
                    rows={3}
                    className="mt-1 focus:ring-emerald-500 focus:border-emerald-500 block w-full sm:text-sm border-gray-300 rounded-md"
                  />
                </div>
              </>
            )}
          </div>
        </div>

        {/* References */}
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">References</h3>
          {[0, 1].map((index) => (
            <div key={index} className="mb-6 last:mb-0">
              <h4 className="text-sm font-medium text-gray-700 mb-4">Reference {index + 1}</h4>
              <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div>
                  <label
                    htmlFor={`references.${index}.name`}
                    className="block text-sm font-medium text-gray-700"
                  >
                    Full Name
                  </label>
                  <input
                    type="text"
                    {...register(`references.${index}.name`)}
                    className="mt-1 focus:ring-emerald-500 focus:border-emerald-500 block w-full sm:text-sm border-gray-300 rounded-md"
                  />
                </div>
                <div>
                  <label
                    htmlFor={`references.${index}.relationship`}
                    className="block text-sm font-medium text-gray-700"
                  >
                    Relationship
                  </label>
                  <input
                    type="text"
                    {...register(`references.${index}.relationship`)}
                    className="mt-1 focus:ring-emerald-500 focus:border-emerald-500 block w-full sm:text-sm border-gray-300 rounded-md"
                  />
                </div>
                <div>
                  <label
                    htmlFor={`references.${index}.phoneNumber`}
                    className="block text-sm font-medium text-gray-700"
                  >
                    Phone Number
                  </label>
                  <input
                    type="tel"
                    {...register(`references.${index}.phoneNumber`)}
                    className="mt-1 focus:ring-emerald-500 focus:border-emerald-500 block w-full sm:text-sm border-gray-300 rounded-md"
                  />
                </div>
                <div>
                  <label
                    htmlFor={`references.${index}.email`}
                    className="block text-sm font-medium text-gray-700"
                  >
                    Email Address
                  </label>
                  <input
                    type="email"
                    {...register(`references.${index}.email`)}
                    className="mt-1 focus:ring-emerald-500 focus:border-emerald-500 block w-full sm:text-sm border-gray-300 rounded-md"
                  />
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Submit Button */}
        <div className="flex justify-end">
          <button
            type="submit"
            disabled={isSubmitting}
            className="inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-emerald-600 hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-emerald-500"
          >
            {isSubmitting ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin mr-2" />
                Submitting...
              </>
            ) : (
              'Submit Application'
            )}
          </button>
        </div>
      </form>
    </div>
  );
};

export default LoanApplication;