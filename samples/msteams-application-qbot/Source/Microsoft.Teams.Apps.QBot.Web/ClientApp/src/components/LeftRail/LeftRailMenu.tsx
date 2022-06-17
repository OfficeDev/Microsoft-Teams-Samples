import React, { useCallback } from 'react';
import { Menu, MenuItem, MenuItemProps } from '@fluentui/react-northstar';
import {
  List as ReactVirtualizedList,
  ListProps as ReactVirtualizedListProps,
  ListRowProps,
} from 'react-virtualized';

export const LeftRailMenuRowHeight = 32;
export type LeftRailMenuProps = Pick<
  ReactVirtualizedListProps,
  'height' | 'rowCount' | 'width'
> & {
  activeIndex: number;
  rowRenderer: (
    Component: typeof MenuItem,
    props: MenuItemProps,
  ) => JSX.Element;
};

export function LeftRailMenu({
  activeIndex,
  rowRenderer,
  ...virtualListProps
}: LeftRailMenuProps): JSX.Element {
  const renderRow = useCallback(
    (props: ListRowProps) => {
      return rowRenderer(MenuItem, {
        active: props.index === activeIndex,
        index: props.index,
        wrapper: {
          ...props.style,
          styles: {
            height: LeftRailMenuRowHeight,
            border: 0,
          },
        },
        styles: {
          border: 0,
          height: props.style.height,
        },
      });
    },
    [rowRenderer, activeIndex],
  );
  return (
    <>
      <Menu
        vertical
        activeIndex={activeIndex}
        pointing="start"
        styles={{ width: virtualListProps.width }}
      >
        <ReactVirtualizedList
          disableHeader
          {...virtualListProps}
          rowRenderer={renderRow}
          // Need to add 2 to the height because of the top/bottom border of the 'a' element created by the MenuItem
          rowHeight={LeftRailMenuRowHeight + 2}
        />
      </Menu>
    </>
  );
}

export default LeftRailMenu;
