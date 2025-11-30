# Design System

A comprehensive design system for the Soar-Fin+ FinTech application, built with React, TypeScript, and Tailwind CSS.

## Features

- 🎨 **Design Tokens**: Centralized color palette, typography, spacing, and more
- 🧩 **Component Library**: Reusable UI components (Button, Input, Card, Modal, Table, Toast)
- 🌓 **Dark Mode**: Full dark mode support with system preference detection
- 📱 **Responsive**: Mobile-first responsive layout system
- ♿ **Accessible**: WCAG 2.1 AA compliant components
- 🎭 **Animations**: Smooth transitions with Framer Motion
- 🧪 **Tested**: Comprehensive unit tests for all components

## Installation

The design system is already integrated into the project. To use it:

```typescript
import { Button, Input, Card, Modal, ThemeProvider } from '@/design-system';
```

## Quick Start

### 1. Wrap your app with ThemeProvider

```typescript
import { ThemeProvider, ToastProvider } from '@/design-system';

function App() {
  return (
    <ThemeProvider>
      <ToastProvider />
      <YourApp />
    </ThemeProvider>
  );
}
```

### 2. Use Components

```typescript
import { Button, Input, Card, toastService } from '@/design-system';

function MyComponent() {
  const handleSubmit = () => {
    toastService.success('Form submitted successfully!');
  };

  return (
    <Card title="User Form" subtitle="Enter your details">
      <Input
        label="Email"
        type="email"
        placeholder="Enter your email"
        required
      />
      <Button variant="primary" onClick={handleSubmit}>
        Submit
      </Button>
    </Card>
  );
}
```

## Components

### Button

```typescript
<Button variant="primary" size="md" loading={false}>
  Click Me
</Button>

// Variants: primary, secondary, outline, ghost, danger
// Sizes: xs, sm, md, lg, xl
```

### Input

```typescript
<Input
  label="Username"
  placeholder="Enter username"
  error="This field is required"
  hint="Choose a unique username"
  icon={<UserIcon />}
  iconPosition="left"
/>
```

### Card

```typescript
<Card
  title="Dashboard"
  subtitle="Overview of your account"
  actions={<Button size="sm">Action</Button>}
  footer={<div>Footer content</div>}
  hoverable
>
  Card content goes here
</Card>
```

### Modal

```typescript
<Modal
  isOpen={isOpen}
  onClose={() => setIsOpen(false)}
  title="Confirm Action"
  description="Are you sure you want to proceed?"
  size="md"
>
  Modal content
</Modal>
```

### Table

```typescript
<Table
  data={users}
  columns={[
    {
      key: 'name',
      header: 'Name',
      accessor: (row) => row.name,
      sortable: true,
    },
    {
      key: 'email',
      header: 'Email',
      accessor: (row) => row.email,
    },
  ]}
  onRowClick={(row) => console.log(row)}
  sortColumn="name"
  sortDirection="asc"
/>
```

### Toast Notifications

```typescript
import { toastService } from '@/design-system';

// Success
toastService.success('Operation completed successfully');

// Error
toastService.error('An error occurred');

// Warning
toastService.warning('Please review your input');

// Info
toastService.info('New update available');

// Promise
toastService.promise(
  fetchData(),
  {
    loading: 'Loading...',
    success: 'Data loaded!',
    error: 'Failed to load data',
  }
);
```

## Layout Components

### Grid System

```typescript
<Grid cols={12} gap="md" responsive={{ sm: 1, md: 2, lg: 3 }}>
  <GridItem span={12} responsive={{ md: 6, lg: 4 }}>
    Column 1
  </GridItem>
  <GridItem span={12} responsive={{ md: 6, lg: 4 }}>
    Column 2
  </GridItem>
  <GridItem span={12} responsive={{ md: 12, lg: 4 }}>
    Column 3
  </GridItem>
</Grid>
```

### Sidebar

```typescript
<Sidebar collapsible defaultCollapsed={false}>
  <SidebarSection title="Navigation">
    <SidebarItem
      icon={<HomeIcon />}
      label="Dashboard"
      active
      onClick={() => navigate('/dashboard')}
    />
    <SidebarItem
      icon={<UsersIcon />}
      label="Users"
      onClick={() => navigate('/users')}
    />
  </SidebarSection>
</Sidebar>
```

### Mobile Navigation

```typescript
const [mobileNavOpen, setMobileNavOpen] = useState(false);

<MobileNavTrigger onClick={() => setMobileNavOpen(true)} />
<MobileNav isOpen={mobileNavOpen} onClose={() => setMobileNavOpen(false)}>
  <nav>Your navigation items</nav>
</MobileNav>
```

### Breadcrumb

```typescript
<Breadcrumb
  items={[
    { label: 'Dashboard', href: '/dashboard' },
    { label: 'Users', href: '/users' },
    { label: 'John Doe' },
  ]}
  showHome
/>
```

## Theme System

### Using Theme Hook

```typescript
import { useTheme } from '@/design-system';

function ThemeToggleButton() {
  const { theme, actualTheme, setTheme, toggleTheme } = useTheme();

  return (
    <button onClick={toggleTheme}>
      Current theme: {actualTheme}
    </button>
  );
}
```

### Theme Toggle Component

```typescript
import { ThemeToggle } from '@/design-system';

<ThemeToggle />
```

## Design Tokens

Access design tokens directly:

```typescript
import { designTokens } from '@/design-system';

const primaryColor = designTokens.colors.primary[600];
const spacing = designTokens.spacing.md;
const fontSize = designTokens.typography.fontSize.lg;
```

## Responsive Breakpoints

- **xs**: 320px (mobile)
- **sm**: 640px (tablet)
- **md**: 768px (tablet landscape)
- **lg**: 1024px (desktop)
- **xl**: 1280px (large desktop)
- **2xl**: 1536px (extra large desktop)

## Dark Mode

Dark mode is automatically applied based on:
1. User preference (set via ThemeToggle)
2. System preference (when theme is set to 'system')

All components support dark mode out of the box using Tailwind's `dark:` variant.

## Testing

Run tests for the design system:

```bash
npm test -- design-system
```

## Best Practices

1. **Always use design tokens** instead of hardcoded values
2. **Prefer composition** over creating new components
3. **Use semantic HTML** for better accessibility
4. **Test components** with different states and props
5. **Follow responsive-first** approach
6. **Maintain consistent spacing** using the spacing scale
7. **Use proper ARIA labels** for accessibility

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Contributing

When adding new components:

1. Create component in `src/design-system/components/`
2. Add TypeScript types
3. Include accessibility features
4. Add dark mode support
5. Write comprehensive tests
6. Update this README
7. Add to Storybook (if available)

## License

Internal use only - Soar-Fin+ FinTech Solution
