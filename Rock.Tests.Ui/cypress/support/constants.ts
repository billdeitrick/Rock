export const RockApiRoutes = {
    blockSettings: "/BlockProperties",
    credentialLogin: "/api/v2/BlockActions/**/CredentialLogin",
    savePrayerRequest: "/api/v2/BlockActions/**/Save"
} as const;

export const RockPageRoutes = {
    externalConnect: "/page/229",
    externalPrayerRequestEntry: "/page/233",
    externalLogin: "/page/207",
} as const;