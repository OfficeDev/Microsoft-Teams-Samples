import { withTeamsThemeProvider } from "../src/utils/TeamsProvider/TeamsProvider";

export const parameters = {
    actions: { argTypesRegex: "^on[A-Z].*" },
    controls: {
        matchers: {
            color: /(background|color)$/i,
            date: /Date$/,
        },
    },
};

export const decorators = [
    (story) =>
        withTeamsThemeProvider({ story }),
];
