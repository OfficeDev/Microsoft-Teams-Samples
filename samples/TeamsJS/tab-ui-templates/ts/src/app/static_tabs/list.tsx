import React from "react";
import {
  Table,
  TableHeader,
  TableHeaderCell,
  TableRow,
  TableCell,
  TableBody,
  TableCellLayout,
  tokens,
  makeStyles,
  useTableFeatures,
  useTableSort,
  TableColumnId,
  TableColumnDefinition,
  createTableColumn,
  Field,
  shorthands,
  Button
} from "@fluentui/react-components";
import { SearchBox } from "@fluentui/react-search-preview";
import {
  MoreHorizontal20Regular,
  Add24Filled
} from "@fluentui/react-icons";

const useStyles = makeStyles({
  root: {
    backgroundColor: tokens.colorSubtleBackgroundHover
  },
  cell: {
    display: 'flex',
    justifyContent: 'center'
  },
  fieldWrapper: {
    ...shorthands.padding(
      tokens.spacingVerticalMNudge,
      tokens.spacingHorizontalMNudge
    ),
  },
});

type MemberNameCell = {
  label: string;
};

type LocationCell = {
  label: string;
};

type RoleCell = {
  label: string;
};

type MenuCell = {
  icon: JSX.Element;
};

type Item = {
  MemberName: MemberNameCell;
  Location: LocationCell;
  Role: RoleCell;
  Menu: MenuCell;
};

const items: Item[] = [
  {
    MemberName: { label: "Babak Shammas (no delete)" },
    Location: { label: "Seattle, WA" },
    Role: { label: "Senior analyst" },
    Menu: {
      icon: <MoreHorizontal20Regular />,
    },
  },
  {
    MemberName: { label: "Aadi Kapoor" },
    Location: { label: "Seattle, WA" },
    Role: { label: "Security associate" },
    Menu: {
      icon: <MoreHorizontal20Regular />,
    },
  },
  {
    MemberName: { label: "Aaron Buxton" },
    Location: { label: "Seattle, WA" },
    Role: { label: "Security engineer: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras in ultricies mi. Sed aliquet odio et magna maximus, et aliquam ipsum faucibus. Sed pulvinar vel nibh eget scelerisque. Vestibulum ornare id felis ut feugiat. Ut vulputate ante non odio condimentum, eget dignissim erat tincidunt. Etiam sodales lobortis viverra. Sed gravida nisi at nisi ornare, non maximus nisi elementum." },
    Menu: {
      icon: <MoreHorizontal20Regular />,
    },
  },
  {
    MemberName: { label: "Alvin Tao (no actions)" },
    Location: { label: "Seattle, WA" },
    Role: { label: "Marketing analyst" },
    Menu: {
      icon: <></>,
    },
  },
  {
    MemberName: { label: "Beth Davies" },
    Location: { label: "Seattle, WA" },
    Role: { label: "Senior engineer" },
    Menu: {
      icon: <MoreHorizontal20Regular />,
    },
  },
];

const columns: TableColumnDefinition<Item>[] = [
  createTableColumn<Item>({
    columnId: "MemberName",
    compare: (a, b) => {
      return a.MemberName.label.localeCompare(b.MemberName.label);
    },
  }),
  createTableColumn<Item>({
    columnId: "Location",
    compare: (a, b) => {
      return a.Location.label.localeCompare(b.Location.label);
    },
  }),
  createTableColumn<Item>({
    columnId: "Role",
    compare: (a, b) => {
      return a.Role.label.localeCompare(b.Role.label);
    },
  }),
  createTableColumn<Item>({
    columnId: ""
  }),
];

export function ListTab() {
  const styles = useStyles();
  const [searchQuery, setSearchQuery] = React.useState("");

  const {
    getRows,
    sort: { getSortDirection, toggleColumnSort, sort },
  } = useTableFeatures(
    {
      columns,
      items,
    },
    [
      useTableSort({
        defaultSortState: { sortColumn: "MemberName", sortDirection: "ascending" },
      }),
    ]
  );

  const headerSortProps = (columnId: TableColumnId) => ({
    onClick: (e: React.MouseEvent) => {
      toggleColumnSort(e, columnId);
    },
    sortDirection: getSortDirection(columnId),
  });

  const rows = sort(getRows());

  const filteredRows = rows.filter((row) =>
    Object.values(row.item).some((cell) => {
      if ('label' in cell) {
        return cell.label.toLowerCase().includes(searchQuery.toLowerCase());
      }
    })
  );

  return (
    <div>
      <div
        style={{
          backgroundColor: "#f4f4f4",
          boxShadow: "0px 4px 8px rgba(0, 0, 0, 0.1)",
          padding: "0px",
          marginBottom: "2px",
          zIndex: 20
        }}
      >

        <div style={{ display: "flex", alignItems: "center" }}>
          <div>
            <Button appearance="transparent" icon={<Add24Filled />}>
              Add
            </Button>
          </div>
          <div className={styles.fieldWrapper} style={{ marginLeft: "auto" }}>
            <Field>
              <SearchBox size="small" placeholder="Search" onChange={(e) => setSearchQuery(e.target.value)} />
            </Field>
          </div>
        </div>

      </div>
      <Table aria-label="List table" className={styles.root}>
        <TableHeader>
          <TableRow>
            {columns.map((column) => (
              <TableHeaderCell {...headerSortProps(column.columnId)}>{column.columnId}</TableHeaderCell>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {filteredRows.map((item, index) => (
            <TableRow key={index}>
              <TableCell>
                <TableCellLayout>
                  {item.item.MemberName.label}
                </TableCellLayout>
              </TableCell>
              <TableCell>
                <TableCellLayout>
                  {item.item.Location.label}
                </TableCellLayout>
              </TableCell>
              <TableCell>
                <TableCellLayout>
                  {item.item.Role.label}
                </TableCellLayout>
              </TableCell>
              <TableCell >
                <TableCellLayout className={styles.cell} media={item.item.Menu.icon}>
                </TableCellLayout>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}