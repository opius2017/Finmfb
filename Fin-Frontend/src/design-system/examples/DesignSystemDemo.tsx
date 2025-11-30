import React, { useState } from 'react';
import {
  Button,
  Input,
  Card,
  Modal,
  Table,
  toastService,
  Grid,
  GridItem,
  Breadcrumb,
  ThemeToggle,
  Sidebar,
  SidebarSection,
  SidebarItem,
} from '../index';
import { Home, Users, Settings, Mail, Search } from 'lucide-react';

/**
 * Design System Demo
 * Showcases all components and their variants
 */
export const DesignSystemDemo: React.FC = () => {
  const [modalOpen, setModalOpen] = useState(false);
  const [loading, setLoading] = useState(false);

  // Sample data for table
  const sampleData = [
    { id: 1, name: 'John Doe', email: 'john@example.com', role: 'Admin' },
    { id: 2, name: 'Jane Smith', email: 'jane@example.com', role: 'User' },
    { id: 3, name: 'Bob Johnson', email: 'bob@example.com', role: 'Manager' },
  ];

  const tableColumns = [
    {
      key: 'name',
      header: 'Name',
      accessor: (row: typeof sampleData[0]) => row.name,
      sortable: true,
    },
    {
      key: 'email',
      header: 'Email',
      accessor: (row: typeof sampleData[0]) => row.email,
      sortable: true,
    },
    {
      key: 'role',
      header: 'Role',
      accessor: (row: typeof sampleData[0]) => (
        <span className="px-2 py-1 text-xs font-medium rounded-full bg-primary-100 text-primary-800 dark:bg-primary-900 dark:text-primary-200">
          {row.role}
        </span>
      ),
    },
  ];

  const handleLoadingDemo = () => {
    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      toastService.success('Operation completed!');
    }, 2000);
  };

  return (
    <div className="min-h-screen bg-neutral-50 dark:bg-neutral-900 p-8">
      <div className="max-w-7xl mx-auto space-y-8">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-4xl font-bold text-neutral-900 dark:text-neutral-100">
              Design System Demo
            </h1>
            <p className="mt-2 text-neutral-600 dark:text-neutral-400">
              Explore all components and their variants
            </p>
          </div>
          <ThemeToggle />
        </div>

        {/* Breadcrumb */}
        <Card>
          <h2 className="text-xl font-semibold mb-4">Breadcrumb Navigation</h2>
          <Breadcrumb
            items={[
              { label: 'Dashboard', onClick: () => console.log('Dashboard') },
              { label: 'Components', onClick: () => console.log('Components') },
              { label: 'Demo' },
            ]}
            showHome
          />
        </Card>

        {/* Buttons */}
        <Card title="Buttons" subtitle="All button variants and sizes">
          <div className="space-y-6">
            <div>
              <h3 className="text-sm font-medium text-neutral-700 dark:text-neutral-300 mb-3">
                Variants
              </h3>
              <div className="flex flex-wrap gap-3">
                <Button variant="primary">Primary</Button>
                <Button variant="secondary">Secondary</Button>
                <Button variant="outline">Outline</Button>
                <Button variant="ghost">Ghost</Button>
                <Button variant="danger">Danger</Button>
              </div>
            </div>

            <div>
              <h3 className="text-sm font-medium text-neutral-700 dark:text-neutral-300 mb-3">
                Sizes
              </h3>
              <div className="flex flex-wrap items-center gap-3">
                <Button size="xs">Extra Small</Button>
                <Button size="sm">Small</Button>
                <Button size="md">Medium</Button>
                <Button size="lg">Large</Button>
                <Button size="xl">Extra Large</Button>
              </div>
            </div>

            <div>
              <h3 className="text-sm font-medium text-neutral-700 dark:text-neutral-300 mb-3">
                States
              </h3>
              <div className="flex flex-wrap gap-3">
                <Button loading={loading} onClick={handleLoadingDemo}>
                  {loading ? 'Loading...' : 'Click to Load'}
                </Button>
                <Button disabled>Disabled</Button>
                <Button icon={<Mail className="h-4 w-4" />} iconPosition="left">
                  With Icon
                </Button>
              </div>
            </div>
          </div>
        </Card>

        {/* Inputs */}
        <Card title="Input Fields" subtitle="Form input components">
          <Grid cols={1} gap="md" responsive={{ md: 2 }}>
            <GridItem>
              <Input
                label="Email"
                type="email"
                placeholder="Enter your email"
                hint="We'll never share your email"
              />
            </GridItem>
            <GridItem>
              <Input
                label="Password"
                type="password"
                placeholder="Enter password"
                required
              />
            </GridItem>
            <GridItem>
              <Input
                label="Search"
                placeholder="Search..."
                icon={<Search className="h-4 w-4" />}
                iconPosition="left"
              />
            </GridItem>
            <GridItem>
              <Input
                label="Error Example"
                error="This field is required"
                placeholder="Invalid input"
              />
            </GridItem>
          </Grid>
        </Card>

        {/* Grid System */}
        <Card title="Grid System" subtitle="Responsive grid layout">
          <Grid cols={1} gap="md" responsive={{ sm: 2, lg: 4 }}>
            {[1, 2, 3, 4].map((num) => (
              <GridItem key={num}>
                <div className="bg-primary-100 dark:bg-primary-900 rounded-lg p-6 text-center">
                  <p className="text-primary-900 dark:text-primary-100 font-medium">
                    Grid Item {num}
                  </p>
                </div>
              </GridItem>
            ))}
          </Grid>
        </Card>

        {/* Table */}
        <Card title="Data Table" subtitle="Sortable table with data">
          <Table
            data={sampleData}
            columns={tableColumns}
            onRowClick={(row) => toastService.info(`Clicked: ${row.name}`)}
          />
        </Card>

        {/* Modal */}
        <Card title="Modal Dialog" subtitle="Click button to open modal">
          <Button onClick={() => setModalOpen(true)}>Open Modal</Button>
          <Modal
            isOpen={modalOpen}
            onClose={() => setModalOpen(false)}
            title="Example Modal"
            description="This is a demo modal dialog"
            size="md"
          >
            <div className="space-y-4">
              <p className="text-neutral-600 dark:text-neutral-400">
                This is the modal content. You can put any React components here.
              </p>
              <div className="flex justify-end gap-3">
                <Button variant="outline" onClick={() => setModalOpen(false)}>
                  Cancel
                </Button>
                <Button
                  variant="primary"
                  onClick={() => {
                    setModalOpen(false);
                    toastService.success('Action confirmed!');
                  }}
                >
                  Confirm
                </Button>
              </div>
            </div>
          </Modal>
        </Card>

        {/* Toast Notifications */}
        <Card title="Toast Notifications" subtitle="Click buttons to see toasts">
          <div className="flex flex-wrap gap-3">
            <Button
              variant="primary"
              onClick={() => toastService.success('Success message!')}
            >
              Success Toast
            </Button>
            <Button
              variant="danger"
              onClick={() => toastService.error('Error message!')}
            >
              Error Toast
            </Button>
            <Button
              variant="secondary"
              onClick={() => toastService.warning('Warning message!')}
            >
              Warning Toast
            </Button>
            <Button
              variant="outline"
              onClick={() => toastService.info('Info message!')}
            >
              Info Toast
            </Button>
          </div>
        </Card>

        {/* Sidebar Demo */}
        <Card title="Sidebar Navigation" subtitle="Collapsible sidebar component">
          <div className="h-64 border border-neutral-200 dark:border-neutral-700 rounded-lg overflow-hidden flex">
            <Sidebar collapsible defaultCollapsed={false}>
              <SidebarSection title="Main">
                <SidebarItem
                  icon={<Home className="h-5 w-5" />}
                  label="Dashboard"
                  active
                />
                <SidebarItem
                  icon={<Users className="h-5 w-5" />}
                  label="Users"
                />
                <SidebarItem
                  icon={<Settings className="h-5 w-5" />}
                  label="Settings"
                />
              </SidebarSection>
            </Sidebar>
            <div className="flex-1 p-6 bg-neutral-50 dark:bg-neutral-800">
              <p className="text-neutral-600 dark:text-neutral-400">
                Main content area
              </p>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
};

export default DesignSystemDemo;
