import { useFluentContext } from '@fluentui/react-northstar';

export type DefaultColorSchemeKey =
  | 'foreground'
  | 'background'
  | 'border'
  | 'shadow'
  | 'foregroundHover'
  | 'backgroundHover'
  | 'borderHover'
  | 'shadowHover'
  | 'foregroundActive'
  | 'backgroundActive'
  | 'borderActive'
  | 'foregroundFocus'
  | 'backgroundFocus'
  | 'borderFocus'
  | 'foregroundPressed'
  | 'backgroundPressed'
  | 'borderPressed'
  | 'foregroundDisabled'
  | 'backgroundDisabled'
  | 'borderDisabled'
  | 'foreground1'
  | 'foreground2'
  | 'foreground3'
  | 'foreground4'
  | 'background1'
  | 'background2'
  | 'background3'
  | 'background4'
  | 'background5'
  | 'border1'
  | 'border2'
  | 'foregroundHover1'
  | 'foregroundHover2'
  | 'backgroundHover1'
  | 'backgroundHover2'
  | 'foregroundPressed1'
  | 'foregroundActive1'
  | 'foregroundActive2'
  | 'backgroundActive1'
  | 'borderActive1'
  | 'borderActive2'
  | 'foregroundFocus1'
  | 'foregroundFocus2'
  | 'foregroundFocus3'
  | 'foregroundFocus4'
  | 'backgroundFocus1'
  | 'backgroundFocus2'
  | 'backgroundFocus3'
  | 'borderFocusWithin'
  | 'borderFocus1'
  | 'foregroundDisabled1'
  | 'backgroundDisabled1';
export function useColorScheme<TKey extends string>(
  schemeName: string,
): Record<TKey, string> {
  const context = useFluentContext();
  return context.theme.siteVariables.colorScheme[schemeName];
}
export function useDefaultColorScheme(): Record<DefaultColorSchemeKey, string> {
  return useColorScheme<DefaultColorSchemeKey>('default');
}
