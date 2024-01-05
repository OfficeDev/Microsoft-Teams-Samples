
import React, { useState, useEffect }  from 'react';
import { 
  FluentProvider, 
  teamsLightTheme, 
  teamsDarkTheme,
  teamsHighContrastTheme,
  DataGrid,
  DataGridHeader,
  DataGridRow,
  DataGridHeaderCell,
  DataGridBody,
  DataGridCell,
  TableCellLayout,
  TableColumnDefinition,
  createTableColumn,
  makeStyles,
  Button 
} from "@fluentui/react-components";
import CompanyBar from  './CompanyBar';
import { Offer } from '../../../common/types';
import { useTeams } from "@microsoft/teamsfx-react";

const useStyles = makeStyles({
  root: {
    minHeight: "100vh",
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'space-between',
  },
  title: {
    fontSize: '40px',
    fontWeight: 'bold',
    marginLeft: '100px',
    marginTop: '100px',
    marginBottom: '100px',
    marginRight: '100px',
  },
  columnTitle: {
    fontWeight: 'bold',
    fontSize: '20px'
  },
  grid: {
    marginLeft: '100px',
    marginRight: '100px',
  }
});

const columns: TableColumnDefinition<Offer>[] = [
  createTableColumn<Offer>({
    columnId: "customer",
    compare: (a, b) => {
      return a.customer.localeCompare(b.customer);
    },
    renderHeaderCell: () => {
      return "Customer";
    },
    renderCell: (item) => {
      return (
        <TableCellLayout>
          {item.customer}
        </TableCellLayout>
      );
    },
  }),
  createTableColumn<Offer>({
    columnId: "salesperson",
    compare: (a, b) => {
      return a.salesperson.localeCompare(b.salesperson);
    },
    renderHeaderCell: () => {
      return "Salesperson";
    },
    renderCell: (item) => {
      return (
          item.salesperson
      );
    },
  }),
  createTableColumn<Offer>({
    columnId: "discountPercentage",
    compare: (a, b) => {
      return a.discountPercentage.toLocaleString().localeCompare(b.discountPercentage.toLocaleString());
    },
    renderHeaderCell: () => {
      return "Discount Percentage";
    },
    renderCell: (item) => {
      return (
          item.discountPercentage
      );
    },
  }),
  createTableColumn<Offer>({
    columnId: "offerText",
    compare: (a, b) => {
      return a.offerText.localeCompare(b.offerText);
    },
    renderHeaderCell: () => {
      return "Offer Text";
    },
    renderCell: (item) => {
      return (
          item.offerText
      );
    },
  }),
]

const App: React.FC = () => {
  const { theme } = useTeams({})[0];

  const classes = useStyles();

  const [items, setDiscounts] = useState<Offer[]>([]);

  const getData = () => { 
    //If you want to run mock APIs with dynamic data, uncomment lines 120 to 124 and comment lines 126 to 142.
    
    //fetch('http://localhost:3001/offers', {cache: "no-store"}) //api for the get request
    //.then(response => response.json())
    //.then(data => {
    // console.log(data);
    //setDiscounts(data);})

    const webData = [
      {
        "id": "fe8cae38-278b-4b56-a8b8-e5a58e565436",
        "customer" : "bob@contoso.com",
        "salesperson": "sally@blueyonderairlines.com",
        "discountPercentage": "15",
        "offerText": "We are pleased to offer you a discount of "
      },
      {
        "id": "d6d0fc50-4715-4fdf-82f2-4bf90d6320c4",
        "customer" : "kim@fabrikam.com",
        "salesperson": "joe@blueyonderairlines.com",
        "discountPercentage": "20",
        "offerText": "As a preferred customer, your discount is"
      }
   ]
   setDiscounts(webData);

  };

  useEffect(() => {
    getData();
  }, []);

  return (
    <FluentProvider
      theme={
        theme || {
          ...teamsDarkTheme,
        }
      }
    >
      <div className={classes.root}>
        <div>
          <div className={classes.title}>Discount Offers</div>
          <Button className={classes.grid} appearance='primary' size='large' onClick={getData}>Refresh data</Button>
          <DataGrid className={classes.grid} items={items} columns={columns} sortable getRowId={(item) => item.customer}>
            <DataGridHeader>
              <DataGridRow>
                {({ renderHeaderCell }) => <DataGridHeaderCell className={classes.columnTitle}>{renderHeaderCell()}</DataGridHeaderCell>}
              </DataGridRow>
            </DataGridHeader>
            <DataGridBody<Offer>>
              {({ item, rowId }) => (
                <DataGridRow<Offer> key={rowId}>
                  {({ renderCell }) => <DataGridCell>{renderCell(item)}</DataGridCell>}
                </DataGridRow>
              )}
            </DataGridBody>
          </DataGrid>
        </div>
        <CompanyBar companyName='Blue Yonder Airlines' />
      </div>
    </FluentProvider>
  );
}

export default App;
