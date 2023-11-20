import React, { useState, useEffect } from 'react';
import { DetailsList, SelectionMode, DefaultButton } from '@fluentui/react';
import { app, call, mail } from '@microsoft/teams-js'
import { Button } from "@fluentui/react-components";
import {
  CallRegular,
  CalendarMailRegular
} from "@fluentui/react-icons";

export const Suppliers = () => {
  const [suppliers, setSuppliers] = useState([]);
  const [selectedSupplier, setSelectedSupplier] = useState(null);

  useEffect(() => {
    async function fetchSuppliers() {
      const response = await fetch("https://services.odata.org/V4/Northwind/Northwind.svc/Suppliers");
      const data = await response.json();
      setSuppliers(data.value);

      try {
        const searchParams = window.location.href;
        await app.initialize();
        const context = await app.getContext();
        if (searchParams.includes('country')) {
          const country = searchParams.match(/=(.*)/)[1];
          const supplier = data.value.filter(x => x.Country === country)
          if (supplier.length > 0) {
            setSuppliers(supplier);
          }
        }
        //deeplinking
        if (context.page.subPageId) {
          const supplier = data.value.filter(x => x.SupplierID === context.page.subPageId)
          if (supplier.length > 0) {
            setSelectedSupplier(supplier[0]);
          }
          else {
            console.error('Supplier not found or invalid data');
          }
        }
      
        
      } catch (error) {
        console.error('Could not initialize Teams JS client library');
      }
    }
    fetchSuppliers();
  }, []);

  const handleRowClick = (supplier) => {
    setSelectedSupplier(supplier);
  };

  async function handleCallButtonClick(item) {
    try {
      await call.startCall({
        targets: [
          'adeleV@m365404404.onmicrosoft.com',
          'admin@m365404404.onmicrosoft.com'
        ],
        requestedModalities: [
          call.CallModalities.Audio,
          call.CallModalities.Video,
          call.CallModalities.VideoBasedScreenSharing,
          call.CallModalities.Data
        ]
      });
    } catch (error) {
      // Handle the error or display an error message
      console.log('An error occurred:', error);
    }
  }
  async function handleMailButtonClick(item){
    try {
      await mail.composeMail({
      type: mail.ComposeMailType.New,
      subject: `Update Needed: ${item.CompanyName}`,
      message: 'Hello',
      toRecipients: [
        'adeleV@m365404404.onmicrosoft.com'
      ]
    });
  } catch (error) {
    // Handle the error or display an error message
    console.log('An error occurred:', error);
  }
}

  const renderContactButton = (item, call, mail) => {
    if (call.isSupported()) {
      return (
        <Button 
          appearance="transparent"
          icon={<CallRegular />}
          onClick={handleCallButtonClick}
        ></Button>
      );
    } else if (mail.isSupported()) {
      return (
        <Button
          appearance="transparent"
          icon={<CalendarMailRegular />}
          onClick={handleMailButtonClick}
        ></Button>
      );
    }
  };

  const supplierColumns = [
    {
      key: 'companyName',
      name: 'Name',
      fieldName: 'CompanyName',
      minWidth: 100,
      maxWidth: 200,
      isResizable: true,
      onRender: (item) => {
        return (
          <a href="javascript:;" key={item.id} onClick={() => handleRowClick(item)}>
            {item.CompanyName}
          </a>
        );
      }
    },
    {
      key: 'contactName',
      name: 'Contact',
      fieldName: 'ContactName',
      minWidth: 100,
      maxWidth: 200,
      isResizable: true
    },
    {
      key: 'contact',
      name: 'Contact',
      fieldName: 'Contact',
      minWidth: 100,
      maxWidth: 200,
      isResizable: true,
      onRender: (item) => {
        return renderContactButton(item, call, mail);
      }
    },
    {
      key: 'country',
      name: 'Country',
      fieldName: 'Country',
      minWidth: 100,
      maxWidth: 200,
      isResizable: true
    }
  ];
  const selectSuppliercolumn = [
    {
      key: 'companyName',
      name: 'Name',
      fieldName: 'CompanyName',
      minWidth: 100,
      maxWidth: 200,
      isResizable: true
    },
    {
      key: 'contactName',
      name: 'Contact',
      fieldName: 'ContactName',
      minWidth: 100,
      maxWidth: 200,
      isResizable: true
    },
    {
      key: 'contact',
      name: 'Contact',
      fieldName: 'Contact',
      minWidth: 100,
      maxWidth: 200,
      isResizable: true,
      onRender: (item) => {
        return renderContactButton(item, call, mail);
      }
    },
    {
      key: 'country',
      name: 'Country',
      fieldName: 'Country',
      minWidth: 100,
      maxWidth: 200,
      isResizable: true
    }
  ];

  return (
    <div>
      {!selectedSupplier && (<div>
        <h2>Suppliers</h2>
        <DetailsList
          items={suppliers}
          columns={supplierColumns}
          selectionMode={SelectionMode.single}
          onItemInvoked={handleRowClick}
        />
      </div>)}
      {selectedSupplier && (
        <div>
          <DetailsList
            items={[selectedSupplier]}
            columns={selectSuppliercolumn}
            selectionMode={SelectionMode.none}
          />
          <DefaultButton key={""} onClick={() => handleRowClick(null)}>
            Back to Suppliers
          </DefaultButton>
        </div>
      )}
    </div>
  );
};