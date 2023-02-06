// Adapted from intlHelper.tsx in the react-intl samples
import * as baseLocale from './locales/en.json';

export type LocaleMessages = typeof baseLocale;
export type LocaleKey = keyof LocaleMessages;

export type SupportedLocale = 'en' | 'fr';

const localeMap: Record<string, SupportedLocale> = {
  en: 'en',
  fr: 'fr',
};
export function parseLocale(
  locale: string,
  defaultLocale: SupportedLocale = 'en',
): SupportedLocale {
  if (!(locale in localeMap)) {
    console.warn('Unknown locale, using default', { locale, defaultLocale });
  }
  return localeMap[locale] ?? defaultLocale;
}
export function loadMessages(locale: SupportedLocale): Promise<LocaleMessages> {
  switch (locale) {
    case 'fr':
      return import('./locales/fr.json');
    case 'en':
      return import('./locales/en.json');
  }
}
