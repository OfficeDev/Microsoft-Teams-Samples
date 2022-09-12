import { useCallback, JSXElementConstructor } from 'react';

export type JsxElementConstructorProps<
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  TComponent extends JSXElementConstructor<any>
> = TComponent extends JSXElementConstructor<infer Props> ? Props : never;

export function useVirtualizedItemRenderer<
  TItem,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  TComponent extends JSXElementConstructor<any>
>(
  items: TItem[],
  render: (
    Component: TComponent,
    props: JsxElementConstructorProps<TComponent> & { index: number },
    item: TItem | undefined,
  ) => JSX.Element,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  deps: any[],
) {
  return useCallback(
    (Component, { index, ...props }) => {
      const maybeItem = items[index];
      return render(Component, { index, ...props }, maybeItem);
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [items, ...deps],
  );
}
