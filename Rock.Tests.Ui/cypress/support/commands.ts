/// <reference types="cypress" />
// ***********************************************
// This example commands.ts shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
//
//
// -- This is a parent command --
// Cypress.Commands.add('login', (email, password) => { ... })
//
//
// -- This is a child command --
// Cypress.Commands.add('drag', { prevSubject: 'element'}, (subject, options) => { ... })
//
//
// -- This is a dual command --
// Cypress.Commands.add('dismiss', { prevSubject: 'optional'}, (subject, options) => { ... })
//
//
// -- This will overwrite an existing command --
// Cypress.Commands.overwrite('visit', (originalFn, url, options) => { ... })
//
// declare global {
//   namespace Cypress {
//     interface Chainable {
//       login(email: string, password: string): Chainable<void>
//       drag(subject: string, options?: Partial<TypeOptions>): Chainable<Element>
//       dismiss(subject: string, options?: Partial<TypeOptions>): Chainable<Element>
//       visit(originalFn: CommandOriginalFn, url: string, options: Partial<VisitOptions>): Chainable<Element>
//     }
//   }
// }
import { RockApiRoutes, RockPageRoutes } from "./constants";

declare global {
  namespace Cypress {
      interface Chainable {
          /**
           * Logs in using username and password.
           * @param username Defaults to "username" environment variable.
           * @param password Defaults to "password" environment variable.
           */
          login: (username?: string | undefined, password?: string | undefined) => void;
          /**
           * Searches for an Obsidian TextBox input by label.
           */
          getObsidianTextBox: (label: string) => Cypress.Chainable<JQuery<HTMLInputElement>>;
          /**
           * Searches for an Obsidian TextBox[multiple=true] textarea by label.
           */
          getObsidianTextArea: (label: string) => Cypress.Chainable<JQuery<HTMLTextAreaElement>>;
          /**
           * Searches for a root Ace editor element by label.
           */
          getAceEditor: (label: string) => Cypress.Chainable<JQuery<HTMLElement>>;
          getAceLibrary: () => Cypress.Chainable<any>;
          /**
           * Sets the value of the ace editor that has a specific label.
           * @param aceLibrary Get this with `getAceLibrary()`.
           * @param label The label text to search for.
           * @param value The new value for the ace editor.
           */
          setAceEditorValue: (aceLibrary: any, label: string, value: string) => Cypress.Chainable<JQuery<any>>;
          /**
           * Searches for an Obsidian DropDown `<select>` element by label, and selects the specified `<option>`.
           */
          selectObsidianDropDownOption: (label: string, option: string) => Cypress.Chainable<JQuery<HTMLElement>>;
          /**
           * Gets the block settings `<iframe>` `<body>` element.
           */
          getBlockSettingsBody: () => Cypress.Chainable<JQuery<HTMLBodyElement>>;
          /**
           * Gets the block settings `<iframe>` `Window`.
           */
          getBlockSettingsWindow: () => Cypress.Chainable<Cypress.AUTWindow>;
      }
  }
}

function getChainableOrWrap(subject) {
  if (!subject) {
    return null;
  } 

  if (Cypress.isCy(subject)) {
    return subject;
  }

  return cy.wrap(subject);
}

function login(username?: string | undefined, password?: string | undefined): void {
  function webformsLogin(internalUsername: string, internalPassword: string) {

    // Set up an HTTP interceptor so we can wait for the login request.
    cy.intercept("POST", `${Cypress.env("baseUrl")}${RockPageRoutes.externalLogin}`).as("credentialLogin");

    // Navigate to the external login page.
    cy.visit(RockPageRoutes.externalLogin);

    // Fill in Username/Password.
    cy.getObsidianTextBox("Username").type(internalUsername);

    // {enter} automatically submits the form so we don't have to search for the button.
    cy.contains("Password").siblings(".control-wrapper").find("input[type=password]").type(`${internalPassword}{enter}`);

    // Wait for the login request to complete before moving on.
    cy.wait("@credentialLogin");

    // Verify that the page redirected to the base URL, indicating a successful login.
    cy.location().should(location => {
      expect(location.origin).to.eq(Cypress.env("baseUrl"));
      expect(location.pathname).to.eq("/");
    });
  }

  function obsidianLogin(internalUsername: string, internalPassword: string) {
    // Set up an HTTP interceptor so we can wait for the login request.
    cy.intercept("POST", `${Cypress.env("baseUrl")}${RockApiRoutes.credentialLogin}`).as("credentialLogin");

    // Navigate to the external login page.
    cy.visit(RockPageRoutes.externalLogin);

    // Uncomment this if the Obsidian Login block is in place to 
    // ensure we switch to Username/Password login mode.
    cy.contains("Sign in with Account").click();

    // Fill in Username/Password.
    cy.getObsidianTextBox("Username").type(internalUsername);

    // {enter} automatically submits the form so we don't have to search for the button.
    cy.contains("Password").siblings(".control-wrapper").find("input[type=password]").type(`${internalPassword}{enter}`);

    // Wait for the login request to complete before moving on.
    cy.wait("@credentialLogin");
  }

  // Using session to cache authentication information so we only login once
  // for the same username/password pair.
  const sessionUsername = username || Cypress.env("username");
  const sessionPassword = password || Cypress.env("password");

  cy.session([sessionUsername, sessionPassword], () => {
    webformsLogin(sessionUsername, sessionPassword);
  });
}
Cypress.Commands.add("login", login);

function getObsidianTextBox(subject: unknown, label: string): Cypress.Chainable<JQuery<HTMLInputElement>> {
  const chainable = getChainableOrWrap(subject) || cy;
    
  return chainable
    .contains(label)
    .siblings(".control-wrapper")
    .find("input");
}
Cypress.Commands.add("getObsidianTextBox", { prevSubject: "optional" }, getObsidianTextBox);

function getObsidianTextArea(subject: unknown, label: string): Cypress.Chainable<JQuery<HTMLTextAreaElement>> {
  const chainable = getChainableOrWrap(subject) || cy;

  return chainable
    .contains(label)
    .siblings(".control-wrapper")
    .find("textarea");
}
Cypress.Commands.add("getObsidianTextArea", { prevSubject: "optional" }, getObsidianTextArea);

function getAceEditor(subject: unknown, label: string): Cypress.Chainable<JQuery<HTMLElement>> {
  const chainable = getChainableOrWrap(subject) || cy;
  return chainable
    .contains(label)
    .siblings(".control-wrapper")
    .find(".ace_editor");
}
Cypress.Commands.add("getAceEditor", { prevSubject: "optional" }, getAceEditor);

function setAceEditorValue(subject: JQuery<any>, aceLibrary: any, label: string, value: string): Cypress.Chainable<JQuery<any>> {
  return cy.wrap(subject).getAceEditor(label)
    .then($aceEditor => {
      const editor = aceLibrary.edit($aceEditor[0]);
      editor.setValue(value);
      return cy.wrap(editor);
    });
}
Cypress.Commands.add("setAceEditorValue", { prevSubject: "element" }, setAceEditorValue);

function getAceLibrary(subject: Window): Cypress.Chainable<any> {
  return cy.wrap(subject)
    .its("ace")
    .should("exist");
}
Cypress.Commands.add("getAceLibrary", { prevSubject: "window" }, getAceLibrary);

function selectObsidianDropDownOption(subject: unknown, label: string, option: string): Cypress.Chainable<JQuery<HTMLElement>> {
  const chainable = getChainableOrWrap(subject) || cy;
  return chainable
    .contains(label)
    .siblings(".control-wrapper")
    .contains(option)
    .then($option => {
      return $option.prop("selected", true).trigger("change");
    });
}
Cypress.Commands.add("selectObsidianDropDownOption", { prevSubject: "optional" }, selectObsidianDropDownOption);

function getBlockSettingsBody(): Cypress.Chainable<JQuery<HTMLBodyElement>> {
  return cy.get("iframe#modal-popup_iframe")
    .its("0.contentDocument.body")
    .should("not.be.empty")
    .then($body => {
      debugger;
      return cy.wrap($body as JQuery<HTMLBodyElement>);
    });
}
Cypress.Commands.add("getBlockSettingsBody", getBlockSettingsBody);

function getBlockSettingsWindow(): Cypress.Chainable<Cypress.AUTWindow> {
  return cy.get("iframe#modal-popup_iframe")
    .its("0.contentWindow")
    .should("not.be.empty")
    .then(win => {
      debugger;
      return cy.wrap(win as Cypress.AUTWindow);
    });
}
Cypress.Commands.add("getBlockSettingsWindow", getBlockSettingsWindow);