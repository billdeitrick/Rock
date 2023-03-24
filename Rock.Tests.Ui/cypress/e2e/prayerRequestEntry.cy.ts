
import { RockApiRoutes, RockPageRoutes } from "../support/constants";

function editBlockSettings(func: () => void) {
  // Skip if nothing to do.
  if (!func) {
    return;
  }

  // Show the block configuration buttons.
  cy.get("#cms-admin-footer .block-config").click();

  cy.get(".prayer-request-entry .block-configuration.config-bar").then($configBar => {
    $configBar.trigger("mouseover");

    cy.wait(500);

    // Open the Prayer Request Entry block settings.
    cy.get(".prayer-request-entry .block-configuration-bar .properties").click();
  });
    
  func();

  // Save the block settings.
  cy.getBlockSettingsBody()
    .find("#btnSave")
    .click();

  // Cannot wait for the POST at this point since the iframe is closed after saving.
  // Instead just wait for the iframe to close.
  cy.getBlockSettingsWindow()
    .should("be.empty");
  
  // Hide the block configuration buttons.
  cy.get("#cms-admin-footer .block-config").click();  
}

describe('Prayer Request Entry', () => {
  function submitPrayerRequest() {
    cy.intercept("POST", `${Cypress.env("baseUrl")}${RockApiRoutes.savePrayerRequest}`).as("save");
    cy.get("button").contains("Save Request").click();
    cy.wait("@save");
  }

  before(() => {
    // One-time block setting configuration goes here.
  });

  beforeEach(() => {
    // Log in with known test credentials.
    // This is a custom command added in the /cypress/support/commands.js file.
    cy.login();

    // Redirect to the external Prayer Request Entry page.
    cy.visit(RockPageRoutes.externalPrayerRequestEntry);

    // Make sure our success template outputs the Prayer Request as JSON.
    editBlockSettings(() => {
      // Navigate To Parent On Save
      cy.getBlockSettingsBody()
        .selectObsidianDropDownOption("Navigate To Parent On Save", "No");

      // Refresh Page On Save
      cy.getBlockSettingsBody()
        .selectObsidianDropDownOption("Refresh Page On Save", "No");

      // Save Success Text
      cy.getBlockSettingsWindow()
        .getAceLibrary()
        .then($ace => {
          cy.getBlockSettingsBody()
            .setAceEditorValue($ace, "Save Success Text", "<pre><code>{{ PrayerRequest | ToJSON }}</code></pre>");
        });

      // Enable Person Matching
      cy.getBlockSettingsBody()
        .selectObsidianDropDownOption("Enable Person Matching", "No");

      // Create Person If No Match Found
      cy.getBlockSettingsBody()
        .selectObsidianDropDownOption("Create Person If No Match Found", "No");

      // Character Limit
      cy.getBlockSettingsBody()
        .getObsidianTextBox("Character Limit")
        .type("{selectAll}250");
    });
  });

  it('Mobile Phone Number and Country Code is saved for new person when entered', () => {
    editBlockSettings(() => {
      cy.getBlockSettingsBody()
        .selectObsidianDropDownOption("Enable Person Matching", "Yes");

      cy.getBlockSettingsBody()
        .selectObsidianDropDownOption("Create Person If No Match Found", "Yes");
    });

    const mobilePhoneNumber = "3605046766";
    const countryCode = "1";

    // Requester fields will be auto-filled with authenticated user info.

    // Override the last name and email so that a new user is created.
    cy.getObsidianTextBox("Last Name").type(Date.now().toString());
    cy.getObsidianTextBox("Email").type(`{moveToStart}${Date.now()}`);

    // Set the request (required field).
    cy.get("label").getObsidianTextArea("Request").type("Pray for peace");

    // Set the mobile phone.
    cy.getObsidianTextBox("Mobile Phone").type(`${countryCode}${mobilePhoneNumber}`);

    // Submit the request.
    submitPrayerRequest();
    
    // Assert that the mobile phone was saved.
    cy.get("pre > code")
      .contains(`"CountryCode": "${countryCode}"`)
      .contains(`"Number": "${mobilePhoneNumber}"`);
  });

  it('Navigates To Parent On Save when block setting enabled', () => {
    // Enable "Navigate To Parent On Save" block setting.
    editBlockSettings(() => {
      cy.getBlockSettingsBody()
        .selectObsidianDropDownOption("Navigate To Parent On Save", "Yes");
    });

    // Requester fields will be auto-filled with authenticated user info.

    // Set the request (required field).
    cy.get("label").getObsidianTextArea("Request").type("Pray for peace");

    // Submit the request.
    submitPrayerRequest();

    // Assert that the page is redirected.
    cy.url().should("include", RockPageRoutes.externalConnect);
  });

  it('Submits request successfully with Request page parameter', () => {
    // Navigate to the Prayer Request Entry page with "Request" page parameter.
    cy.visit(`${RockPageRoutes.externalPrayerRequestEntry}?Request=Pray 1234`);
  
    // Requester fields will be auto-filled with authenticated person info.

    // Submit the request.
    submitPrayerRequest();
    
    // Assert the save was successful.
    cy.get("pre > code").contains("Pray 1234");
  });

  it('Refreshes page on save when Refresh Page On Save is enabled and Navigate To Parent On Save is disabled', () => {
    // Disable "Navigate To Parent On Save" block setting.
    editBlockSettings(() => {
      // Enable "Refresh Page On Save" block setting.
      cy.getBlockSettingsBody()
        .selectObsidianDropDownOption("Refresh Page On Save", "Yes");
    });

    // Navigate to the Prayer Request Entry page with "Request" page parameter.
    cy.visit(`${RockPageRoutes.externalPrayerRequestEntry}?Request=Pray for ${Date.now()}`);

    // Store the current URL to verify at the end.
    cy.url().then($url => {
      // Requester fields will be auto-filled with authenticated person info.

      // Submit the request.
      submitPrayerRequest();
      
      // Assert the success message is not visible because the page refreshed.
      cy.get("pre > code").should("not.exist");

      // Assert that the URL is the same.
      cy.url().should("eq", $url);
    });
  });

  it("Enables the entry form with First Name, Last Name, Email, Mobile Phone, Category in tact after clicking 'Add Another Request'", () => {

    // Navigate to the Prayer Request Entry page with "Request" page parameter.
    cy.visit(`${RockPageRoutes.externalPrayerRequestEntry}?Request=Pray for ${Date.now()}`);

    // Requester fields will be auto-filled with authenticated person info.

    cy.getObsidianTextBox("First Name").then($firstName => {
      cy.getObsidianTextBox("Last Name").then($lastName => {
        cy.getObsidianTextBox("Email").then($email => {
          cy.getObsidianTextBox("Mobile Phone").then($mobilePhone => {
            const firstName = $firstName.val();
            const lastName = $lastName.val();
            const email = $email.val();
            const mobilePhone = $mobilePhone.val();
            
            // Submit the request.
            submitPrayerRequest();

            // Click 'Add Another Request' to show the form again.
            cy.contains("Add Another Request").click();

            // Assert that the inputs still contain the same values.
            cy.getObsidianTextBox("First Name").should("have.value", firstName);
            cy.getObsidianTextBox("Last Name").should("have.value", lastName);
            cy.getObsidianTextBox("Email").should("have.value", email);
            cy.getObsidianTextBox("Mobile Phone").should("have.value", mobilePhone);
          });
        });
      });
    });
  });
  
  it("Clicking 'Add Another Request' should set focus to the Request field", () => {

    // Navigate to the Prayer Request Entry page with "Request" page parameter.
    cy.visit(`${RockPageRoutes.externalPrayerRequestEntry}?Request=Pray for ${Date.now()}`);

    // Requester fields will be auto-filled with authenticated person info.

    // Submit the request.
    submitPrayerRequest();

    // Click 'Add Another Request' to show the form again.
    cy.contains("Add Another Request").click();

    cy.get("label").getObsidianTextArea("Request").then($request => {
      cy.focused().then($el => 
        cy.wrap($el[0] === $request[0]).should("be.true")
      );
    });
  });
  
  it("Request field should be limited to Character Limit block setting", () => {
    editBlockSettings(() => {
      cy.getBlockSettingsBody()
        .getObsidianTextBox("Character Limit")
        .type("{selectAll}10");
    });

    // Navigate to the Prayer Request Entry page with "Request" page parameter.
    cy.visit(`${RockPageRoutes.externalPrayerRequestEntry}?Request=Pray for ${Date.now()}`);

    // Requester fields will be auto-filled with authenticated person info.

    // Submit the request.
    submitPrayerRequest();

    // Click 'Add Another Request' to show the form again.
    cy.contains("Whoops. Would you mind reducing the length of your prayer request to 10 characters?").should("exist");
  });
});