import React from 'react';
import { Building2 } from 'lucide-react';

interface LogoProps {
  size?: 'sm' | 'md' | 'lg' | 'xl';
  showText?: boolean;
  variant?: 'default' | 'white' | 'dark';
  className?: string;
}

const Logo: React.FC<LogoProps> = ({ 
  size = 'md', 
  showText = true, 
  variant = 'default',
  className = '' 
}) => {
  const sizeClasses = {
    sm: 'w-6 h-6',
    md: 'w-8 h-8',
    lg: 'w-12 h-12',
    xl: 'w-16 h-16'
  };

  const textSizeClasses = {
    sm: 'text-lg',
    md: 'text-xl',
    lg: 'text-2xl',
    xl: 'text-3xl'
  };

  const iconSizeClasses = {
    sm: 'w-3 h-3',
    md: 'w-4 h-4',
    lg: 'w-6 h-6',
    xl: 'w-8 h-8'
  };

  const plusSizeClasses = {
    sm: 'w-2 h-2 text-xs',
    md: 'w-3 h-3 text-xs',
    lg: 'w-4 h-4 text-sm',
    xl: 'w-5 h-5 text-base'
  };

  const getVariantClasses = () => {
    switch (variant) {
      case 'white':
        return {
          container: 'bg-white text-emerald-600',
          plus: 'bg-emerald-600 text-white',
          text: 'text-white'
        };
      case 'dark':
        return {
          container: 'bg-gray-800 text-emerald-400',
          plus: 'bg-emerald-400 text-gray-800',
          text: 'text-gray-800'
        };
      default:
        return {
          container: 'bg-gradient-to-br from-emerald-600 to-emerald-700 text-white',
          plus: 'bg-yellow-400 text-emerald-800',
          text: 'text-emerald-600'
        };
    }
  };

  const variantClasses = getVariantClasses();

  return (
    <div className={`flex items-center space-x-3 ${className}`}>
      {/* Logo Icon */}
      <div className={`${sizeClasses[size]} ${variantClasses.container} rounded-2xl flex items-center justify-center shadow-lg relative`}>
        <div className="relative">
          <Building2 className={iconSizeClasses[size]} />
          {/* Plus Symbol */}
          <div className={`absolute -top-1 -right-1 ${plusSizeClasses[size]} ${variantClasses.plus} rounded-full flex items-center justify-center font-bold shadow-sm`}>
            +
          </div>
        </div>
      </div>

      {/* Logo Text */}
      {showText && (
        <div>
          <h1 className={`${textSizeClasses[size]} font-bold bg-gradient-to-r from-emerald-600 to-emerald-800 bg-clip-text text-transparent`}>
            Soar-Fin+
          </h1>
          {size !== 'sm' && (
            <p className={`text-xs ${variantClasses.text} font-medium opacity-80`}>
              Enterprise FinTech
            </p>
          )}
        </div>
      )}
    </div>
  );
};

export default Logo;