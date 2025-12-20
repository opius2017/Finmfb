describe('Quick Actions E2E', () => {
    beforeEach(() => {
        // Login before each test
        cy.visit('/login');
        cy.get('input[name="email"]').type('admin@soarmfb.ng');
        cy.get('input[name="password"]').type('@Searhealth123');
        cy.get('button[type="submit"]').click();

        // Wait for dashboard to load
        cy.url().should('include', '/dashboard');
        cy.contains('Quick Actions').should('be.visible');
    });

    it('should navigate to Customers page when Add Customer is clicked', () => {
        cy.contains('button', 'Add Customer').click();
        cy.url().should('include', '/customers');
    });

    it('should navigate to Deposits page when Open Account is clicked', () => {
        cy.contains('button', 'Open Account').click();
        cy.url().should('include', '/dashboard/deposits');
    });

    it('should navigate to Deposits page when Process Transaction is clicked', () => {
        cy.contains('button', 'Process Transaction').click();
        cy.url().should('include', '/dashboard/deposits');
    });

    it('should navigate to Financial Reports page when View Reports is clicked', () => {
        cy.contains('button', 'View Reports').click();
        cy.url().should('include', '/financial-reports');
    });
});
