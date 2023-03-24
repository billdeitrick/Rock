import { defineConfig } from "cypress";

const baseUrl = "http://localhost:6229";
//const baseUrl = "https://prealpha.rocksolidchurchdemo.com";

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
    
    // Using 15 seconds as the default time to wait until most DOM based commands are considered timed out.
    // Increase/decrease this based on your system's performance.
    defaultCommandTimeout: 15000,

    setupNodeEvents(on, config) {
      // implement node event listeners here
    }
  }
});
