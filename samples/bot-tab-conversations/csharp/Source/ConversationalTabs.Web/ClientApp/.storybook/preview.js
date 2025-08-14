import { withTeamsThemeProvider } from "utils/MockTeamsProvider";

export const parameters = {
  actions: { argTypesRegex: '^on[A-Z].*' },
  controls: {
    matchers: {
      color: /(background|color)$/i,
      date: /Date$/,
    },
  },
};

export const globalTypes = {
  theme: {
    name: 'Teams Theme',
    description: 'Teams theme for components',
    defaultValue: 'default',
    toolbar: {
      icon: 'lightning',
      items: ['default', 'dark', 'contrast'],
      showName: true,
    },
  },
};

export const decorators = [(story, context) => withTeamsThemeProvider({ story, context })];
