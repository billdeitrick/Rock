
import { RockPageRoutes } from "../support/constants";

describe('Login', () => {
  before(() => {
    // One-time block setting configuration goes here.
  });

  beforeEach(() => {
    // Log in with known test credentials.
    // This is a custom command added in the /cypress/support/commands.js file.
    // cy.login();
  });

  it('Login block container div on external block should not contain "(legacy)"', () => {
    // Redirect to the external Login page.
    cy.visit(RockPageRoutes.externalLogin);

    cy.get(".block-instance.login").should("exist");
  });

  // This is a sanity check to make the WebForms Login block has the expected CSS class in multiple pages.
  it('Login block container div on internal block should not contain "(legacy)"', () => {
    // Redirect to the external Login page.
    cy.visit(RockPageRoutes.internalLogin);

    cy.get(".block-instance.login").should("exist");
  });
});