import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import {
    Calculator,
    User,
    Users,
    FileText,
    CheckCircle,
    TrendingUp,
    ArrowRight,
    ArrowLeft,
    Upload,
    Shield,
    Wallet
} from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { LoanCalculator } from './LoanCalculator';
import { LoanType } from '../types/loan.types';
import toast from 'react-hot-toast';

const steps = [
    { id: 1, title: 'Calculator', icon: Calculator, desc: 'Check eligibility' },
    { id: 2, title: 'Personal', icon: User, desc: 'Your details' },
    { id: 3, title: 'Guarantors', icon: Users, desc: 'Add security' },
    { id: 4, title: 'Collateral', icon: Wallet, desc: 'Assets & Docs' },
    // { id: 5, title: 'Schedule', icon: Calendar, desc: 'Repayment plan' }, // Merged into Review for brevity or keep separate
    { id: 5, title: 'Review', icon: CheckCircle, desc: 'Submit' },
];

const NewLoanWizard = () => {
    const navigate = useNavigate();
    const [currentStep, setCurrentStep] = useState(1);
    const [loading, setLoading] = useState(false);

    // Form State
    const [loanData, setLoanData] = useState({
        amount: 100000,
        tenor: 12,
        loanType: 'normal' as LoanType,
        monthlyEMI: 0,
        totalInterest: 0,
    });

    const [personalInfo, setPersonalInfo] = useState({
        name: 'Opius User', // Mock logged in user
        id: 'MEMBER-2024-001',
        income: 250000,
    });

    const [guarantors, setGuarantors] = useState([
        { id: 1, name: 'Sample Guarantor', status: 'Pending Notification' } // Mock initial
    ]);

    const [collateralDocs, setCollateralDocs] = useState<File[]>([]);

    // Handlers
    const nextStep = () => setCurrentStep(prev => Math.min(prev + 1, steps.length));
    const prevStep = () => setCurrentStep(prev => Math.max(prev - 1, 1));

    const handleCalculatorComplete = (data: any) => {
        setLoanData({
            amount: data.principal,
            tenor: data.tenor,
            loanType: data.loanType,
            monthlyEMI: data.monthlyEMI,
            totalInterest: data.totalInterest,
        });
    };

    const addGuarantor = () => {
        // Logic to open modal/search would go here
        const newG = { id: Date.now(), name: 'New Guarantor', status: 'Pending Notification' };
        setGuarantors([...guarantors, newG]);
        toast.success('Guarantor added');
    };

    const removeGuarantor = (id: number) => {
        setGuarantors(guarantors.filter(g => g.id !== id));
    };

    const handleSubmit = async () => {
        setLoading(true);
        // Simulate API call
        setTimeout(() => {
            setLoading(false);
            toast.success('Loan Application Submitted Successfully!');
            navigate('/loans/applications');
        }, 2000);
    };

    const renderStepContent = () => {
        switch (currentStep) {
            case 1: // Calculator
                return (
                    <motion.div initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }}>
                        <LoanCalculator
                            onCalculationComplete={handleCalculatorComplete}
                            initialValues={{ amount: loanData.amount, tenor: loanData.tenor, loanType: loanData.loanType }}
                        />
                    </motion.div>
                );

            case 2: // Personal Info
                return (
                    <motion.div initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} className="space-y-6">
                        <h2 className="text-2xl font-bold text-gray-900">Borrower Information</h2>
                        <div className="bg-blue-50 p-4 rounded-xl border border-blue-100 flex items-center mb-6">
                            <User className="w-6 h-6 text-blue-600 mr-3" />
                            <div>
                                <h4 className="font-bold text-blue-900">Member Profile</h4>
                                <p className="text-sm text-blue-700">Details auto-filled from your membership record.</p>
                            </div>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div className="space-y-1">
                                <label className="text-sm font-medium text-gray-700">Full Name</label>
                                <input type="text" value={personalInfo.name} disabled className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-gray-500" />
                            </div>
                            <div className="space-y-1">
                                <label className="text-sm font-medium text-gray-700">Member ID</label>
                                <input type="text" value={personalInfo.id} disabled className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-gray-500" />
                            </div>
                            <div className="space-y-1">
                                <label className="text-sm font-medium text-gray-700">Verified Monthly Income</label>
                                <input type="text" value={`₦${personalInfo.income.toLocaleString()}`} disabled className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-gray-500 font-bold" />
                            </div>
                        </div>
                    </motion.div>
                );

            case 3: // Guarantors
                return (
                    <motion.div initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} className="space-y-6">
                        <div className="flex justify-between items-center">
                            <h2 className="text-2xl font-bold text-gray-900">Guarantors</h2>
                            <button onClick={addGuarantor} className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700">
                                + Add Guarantor
                            </button>
                        </div>

                        {guarantors.length === 0 ? (
                            <div className="text-center py-12 bg-gray-50 rounded-xl border-dashed border-2 border-gray-200">
                                <Users className="w-12 h-12 text-gray-300 mx-auto mb-2" />
                                <p className="text-gray-500">No guarantors added yet. Recommended: 2</p>
                            </div>
                        ) : (
                            <div className="grid gap-4">
                                {guarantors.map(g => (
                                    <div key={g.id} className="flex items-center justify-between p-4 bg-white border border-gray-200 rounded-xl shadow-sm">
                                        <div className="flex items-center space-x-3">
                                            <div className="w-10 h-10 bg-indigo-100 rounded-full flex items-center justify-center text-indigo-600 font-bold">
                                                {g.name[0]}
                                            </div>
                                            <div>
                                                <p className="font-bold text-gray-900">{g.name}</p>
                                                <p className="text-xs text-amber-600 font-medium bg-amber-50 px-2 py-0.5 rounded inline-block mt-1">{g.status}</p>
                                            </div>
                                        </div>
                                        <button onClick={() => removeGuarantor(g.id)} className="text-red-500 hover:bg-red-50 p-2 rounded-lg text-sm">Remove</button>
                                    </div>
                                ))}
                            </div>
                        )}

                        <div className="bg-amber-50 p-4 rounded-lg border border-amber-100 text-sm text-amber-800">
                            Starting this application will send notifications to selected guarantors to approve their liability.
                        </div>
                    </motion.div>
                );

            case 4: // Collateral
                return (
                    <motion.div initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} className="space-y-6">
                        <h2 className="text-2xl font-bold text-gray-900">Collateral & Security</h2>

                        {/* Automated Collateral */}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div className="p-5 bg-emerald-50 rounded-xl border border-emerald-100">
                                <div className="flex items-center space-x-3 mb-2">
                                    <Wallet className="w-5 h-5 text-emerald-600" />
                                    <h4 className="font-bold text-emerald-900">Savings Balance</h4>
                                </div>
                                <p className="text-2xl font-bold text-emerald-700">₦{(loanData.amount * 0.3).toLocaleString()}</p>
                                <p className="text-xs text-emerald-600 mt-1">Available as security (30% of Loan)</p>
                            </div>
                            <div className="p-5 bg-blue-50 rounded-xl border border-blue-100">
                                <div className="flex items-center space-x-3 mb-2">
                                    <Users className="w-5 h-5 text-blue-600" />
                                    <h4 className="font-bold text-blue-900">Guarantor Coverage</h4>
                                </div>
                                <p className="text-2xl font-bold text-blue-700">₦{(loanData.amount * 0.7).toLocaleString()}</p>
                                <p className="text-xs text-blue-600 mt-1">Covered by {guarantors.length} guarantors</p>
                            </div>
                        </div>

                        {/* Optional Docs */}
                        <div className="mt-8">
                            <h3 className="font-bold text-gray-900 mb-4">Additional Documents (Optional)</h3>
                            <div
                                className="border-2 border-dashed border-gray-300 rounded-xl p-8 text-center hover:border-indigo-500 hover:bg-indigo-50 transition-all cursor-pointer"
                                onClick={() => document.getElementById('file-upload')?.click()}
                            >
                                <Upload className="w-10 h-10 text-gray-400 mx-auto mb-3" />
                                <h4 className="font-medium text-gray-900">Upload Property Documents</h4>
                                <p className="text-sm text-gray-500 mt-1">If you wish to add physical collateral (C of O, etc)</p>
                                <input id="file-upload" type="file" className="hidden" multiple onChange={(e) => setCollateralDocs(Array.from(e.target.files || []))} />
                            </div>
                            {collateralDocs.length > 0 && (
                                <div className="mt-4 space-y-2">
                                    {collateralDocs.map((doc, idx) => (
                                        <div key={idx} className="flex items-center text-sm text-gray-600 bg-gray-50 px-3 py-2 rounded">
                                            <FileText className="w-4 h-4 mr-2" />
                                            {doc.name}
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </motion.div>
                );

            case 5: // Review
                return (
                    <motion.div initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} className="space-y-6">
                        <div className="text-center mb-8">
                            <h2 className="text-2xl font-bold text-gray-900">Review Application</h2>
                            <p className="text-gray-500">Please confirm all details before submitting.</p>
                        </div>

                        <div className="bg-white border boundary-gray-200 rounded-2xl shadow-sm overflow-hidden">
                            <div className="p-6 bg-gray-50 border-b border-gray-100">
                                <div className="flex justify-between items-center">
                                    <div>
                                        <p className="text-sm text-gray-500">Total Loan Amount</p>
                                        <p className="text-3xl font-extrabold text-indigo-900">₦{loanData.amount.toLocaleString()}</p>
                                    </div>
                                    <div className="text-right">
                                        <p className="text-sm text-gray-500">Monthly Repayment</p>
                                        <p className="text-xl font-bold text-indigo-600">₦{loanData.monthlyEMI.toLocaleString(undefined, { maximumFractionDigits: 0 })}</p>
                                    </div>
                                </div>
                            </div>

                            <div className="p-6 space-y-4">
                                <div className="flex justify-between border-b border-gray-100 pb-2">
                                    <span className="text-gray-600">Loan Product</span>
                                    <span className="font-medium capitalize">{loanData.loanType} Loan</span>
                                </div>
                                <div className="flex justify-between border-b border-gray-100 pb-2">
                                    <span className="text-gray-600">Tenor</span>
                                    <span className="font-medium">{loanData.tenor} Months</span>
                                </div>
                                <div className="flex justify-between border-b border-gray-100 pb-2">
                                    <span className="text-gray-600">Total Interest</span>
                                    <span className="font-medium">₦{loanData.totalInterest.toLocaleString(undefined, { maximumFractionDigits: 0 })}</span>
                                </div>
                                <div className="flex justify-between pt-2">
                                    <span className="text-gray-600">Guarantors</span>
                                    <span className="font-medium">{guarantors.length} Selected</span>
                                </div>
                            </div>
                        </div>

                        <div className="flex items-start space-x-3 p-4 bg-gray-50 rounded-xl">
                            <input type="checkbox" className="mt-1 w-5 h-5 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500" />
                            <p className="text-sm text-gray-600">
                                I agree to the <span className="text-indigo-600 font-medium cursor-pointer">Terms and Conditions</span>, and I authorize FinMFB to deduct monthly repayments from my salary/savings account.
                            </p>
                        </div>
                    </motion.div>
                );
            default: return null;
        }
    };

    return (
        <div className="max-w-5xl mx-auto py-8 px-4">
            {/* Stepper */}
            <div className="mb-10">
                <div className="flex items-center justify-between relative z-10">
                    {steps.map((step) => {
                        const isActive = step.id === currentStep;
                        const isCompleted = step.id < currentStep;
                        return (
                            <div key={step.id} className="flex flex-col items-center flex-1 cursor-pointer" onClick={() => step.id < currentStep && setCurrentStep(step.id)}>
                                <div className={`w-10 h-10 rounded-full flex items-center justify-center border-2 transition-all ${isActive ? 'bg-indigo-600 border-indigo-600 text-white shadow-lg scale-110' : isCompleted ? 'bg-green-500 border-green-500 text-white' : 'bg-white border-gray-300 text-gray-400'}`}>
                                    {isCompleted ? <CheckCircle className="w-6 h-6" /> : <step.icon className="w-5 h-5" />}
                                </div>
                                <span className={`mt-2 text-xs font-bold uppercase tracking-wider ${isActive ? 'text-indigo-600' : 'text-gray-400'}`}>{step.title}</span>
                            </div>
                        );
                    })}
                </div>
                {/* Progress Bar */}
                <div className="relative -mt-8 mx-12 h-0.5 bg-gray-200 -z-0">
                    <div className="absolute top-0 left-0 h-full bg-green-500 transition-all duration-300" style={{ width: `${((currentStep - 1) / (steps.length - 1)) * 100}%` }}></div>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
                {/* Main Content */}
                <div className="lg:col-span-12">
                    <div className="bg-white rounded-3xl shadow-xl border border-gray-100 p-8 min-h-[500px] relative">
                        {renderStepContent()}

                        {/* Navigation */}
                        <div className="flex justify-between items-center mt-12 pt-6 border-t border-gray-100">
                            <button
                                onClick={prevStep}
                                disabled={currentStep === 1}
                                className={`flex items-center px-6 py-3 rounded-xl font-medium transition-all ${currentStep === 1 ? 'opacity-0 pointer-events-none' : 'text-gray-600 hover:bg-gray-50'}`}
                            >
                                <ArrowLeft className="w-5 h-5 mr-2" /> Back
                            </button>

                            <button
                                onClick={currentStep === steps.length ? handleSubmit : nextStep}
                                disabled={loading}
                                className="flex items-center px-8 py-3 bg-indigo-600 text-white rounded-xl shadow-lg shadow-indigo-200 hover:bg-indigo-700 hover:-translate-y-0.5 transition-all font-bold disabled:opacity-70 disabled:cursor-not-allowed"
                            >
                                {loading ? 'Processing...' : currentStep === steps.length ? 'Submit Application' : 'Continue'}
                                {!loading && <ArrowRight className="w-5 h-5 ml-2" />}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default NewLoanWizard;
