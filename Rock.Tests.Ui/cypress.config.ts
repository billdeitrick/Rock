import { defineConfig } from "cypress";

const baseUrl = "http://localhost:6229";

export default defineConfig({
  projectId: 'hkcrq3',
  env: {
    baseUrl,

    // TODO Credentials should be moved somewhere else.
    username: "admin",
    password: "admin"
  },
  e2e: {
    baseUrl,
    defaultCommandTimeout: 60000,
    setupNodeEvents(on, config) {
      // implement node event listeners here
    }
  }
});
