import React, { Fragment } from 'react';
import { Dialog, Transition } from '@headlessui/react';
import { X, Menu } from 'lucide-react';
import { clsx } from 'clsx';

export interface MobileNavProps {
  isOpen: boolean;
  onClose: () => void;
  children: React.ReactNode;
}

export const MobileNav: React.FC<MobileNavProps> = ({ isOpen, onClose, children }) => {
  return (
    <Transition.Root show={isOpen} as={Fragment}>
      <Dialog as="div" className="relative z-50 lg:hidden" onClose={onClose}>
        <Transition.Child
          as={Fragment}
          enter="transition-opacity ease-linear duration-300"
          enterFrom="opacity-0"
          enterTo="opacity-100"
          leave="transition-opacity ease-linear duration-300"
          leaveFrom="opacity-100"
          leaveTo="opacity-0"
        >
          <div className="fixed inset-0 bg-neutral-900/80 backdrop-blur-sm" />
        </Transition.Child>

        <div className="fixed inset-0 flex">
          <Transition.Child
            as={Fragment}
            enter="transition ease-in-out duration-300 transform"
            enterFrom="-translate-x-full"
            enterTo="translate-x-0"
            leave="transition ease-in-out duration-300 transform"
            leaveFrom="translate-x-0"
            leaveTo="-translate-x-full"
          >
            <Dialog.Panel className="relative mr-16 flex w-full max-w-xs flex-1">
              <Transition.Child
                as={Fragment}
                enter="ease-in-out duration-300"
                enterFrom="opacity-0"
                enterTo="opacity-100"
                leave="ease-in-out duration-300"
                leaveFrom="opacity-100"
                leaveTo="opacity-0"
              >
                <div className="absolute left-full top-0 flex w-16 justify-center pt-5">
                  <button
                    type="button"
                    className="-m-2.5 p-2.5 text-white hover:text-neutral-300 focus:outline-none focus:ring-2 focus:ring-white rounded-lg"
                    onClick={onClose}
                    aria-label="Close sidebar"
                  >
                    <X className="h-6 w-6" aria-hidden="true" />
                  </button>
                </div>
              </Transition.Child>

              <div className="flex grow flex-col gap-y-5 overflow-y-auto bg-white dark:bg-neutral-900 px-6 pb-4 ring-1 ring-neutral-900/10 dark:ring-neutral-100/10">
                {children}
              </div>
            </Dialog.Panel>
          </Transition.Child>
        </div>
      </Dialog>
    </Transition.Root>
  );
};

export interface MobileNavTriggerProps {
  onClick: () => void;
  className?: string;
}

export const MobileNavTrigger: React.FC<MobileNavTriggerProps> = ({ onClick, className }) => {
  return (
    <button
      type="button"
      className={clsx(
        'lg:hidden -m-2.5 inline-flex items-center justify-center rounded-md p-2.5',
        'text-neutral-700 dark:text-neutral-300',
        'hover:bg-neutral-100 dark:hover:bg-neutral-800',
        'focus:outline-none focus:ring-2 focus:ring-primary-500',
        className
      )}
      onClick={onClick}
      aria-label="Open sidebar"
    >
      <Menu className="h-6 w-6" aria-hidden="true" />
    </button>
  );
};
