import React, { useState } from "react";
import { v4 as uuidv4 } from "uuid";
import {
  Field,
  Input,
  Textarea,
  Button,
  makeStyles,
  FluentProvider,
  webLightTheme,
  tokens,
  Theme,
  webDarkTheme,
} from "@fluentui/react-components";
import CompanyBar from "./CompanyBar";
import { Offer } from "../../../../common/types";

/* global console, Office, fetch */

const useStyles = makeStyles({
  root: {
    display: "flex",
    flexDirection: "column",
    justifyContent: "space-between",
    color: tokens.colorBrandForeground2,
    minHeight: "100vh",
    minWidth: "100vw",
    // The next five lines are to counteract the fact that Office adds
    // a margin of 8 px just inside the margins of the task pane.
    // This area is always white, even when the Office theme is dark.
    // It looks bad. So we have to add a background color and
    // extend the add-in page's margins out to cover the area
    // with negative margin values.
    backgroundColor: tokens.colorNeutralStrokeOnBrand,
    marginTop: "-8px",
    marginLeft: "-8px",
    marginBottom: "-8px",
    marginRight: "-8px",
  },
  inputField: {
    marginLeft: "20px",
    marginTop: "30px",
    marginBottom: "20px",
    marginRight: "20px",
    maxWidth: "20px",
  },
  textAreaField: {
    marginLeft: "20px",
    marginTop: "30px",
    marginBottom: "20px",
    marginRight: "20px",
    maxWidth: "50%",
  },
  button: {
    maxWidth: "fit-content",
    marginLeft: "20px",
  },
});

const App: React.FC = () => {
  const [discount, setDiscount] = useState("15");
  const [offerText, setOfferText] = useState("We are delighted to offer you a discount of");
  const styles = useStyles();

  const handleDiscountChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const valueAsNumeral = event.target.value;
    const valueAsNumber = Number(valueAsNumeral);
    if (valueAsNumber >= 0 && valueAsNumber <= 100) {
      setDiscount(valueAsNumeral);
    }
  };

  const handleOfferTextChange = async (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    setOfferText(event.target.value);
  };

  const setDiscountText = (): Promise<void> => {
    return new Promise(function (resolve, reject) {
      try {
        Office.context.mailbox.item.setSelectedDataAsync(offerText + " " + discount + "%.", function (asyncResult) {
          if (asyncResult.status === Office.AsyncResultStatus.Succeeded) {
            console.log("Selected text has been updated successfully.");
            resolve();
          } else {
            console.error(asyncResult.error);
            reject(asyncResult.error);
          }
        });
      } catch (error) {
        reject(error);
      }
    });
  };

  const getCustomerName = (): Promise<string> => {
    return new Promise(function (resolve, reject) {
      try {
        let customerName: string = "";
        Office.context.mailbox.item.to.getAsync((asyncResult) => {
          if (asyncResult.status === Office.AsyncResultStatus.Succeeded) {
            const msgTo = asyncResult.value;
            customerName = msgTo[0].displayName !== "" ? msgTo[0].displayName : msgTo[0].emailAddress;
            resolve(customerName);
          } else {
            console.error(asyncResult.error);
            reject(asyncResult.error);
          }
        });
      } catch (error) {
        reject(error);
      }
    });
  };

  const handleInsertOffer = async () => {
    const salesPerson: string = Office.context.mailbox.userProfile.displayName;
    await setDiscountText();
    const customerName = await getCustomerName();

    const offer: Offer = {
      customer: customerName,
      salesperson: salesPerson,
      discountPercentage: discount,
      offerText: offerText,
    };
    addOfferToCRM(offer);
  };

  /*
    The Office.context.officeTheme object does not have a name
    or ID property, so the theme must be inferred from one of
    the color properties. If the foreground color (i.e., text color)
    is off-white (f0f0f0), assume a dark Office theme. Otherwise,
    assume a light Office theme.
  */
  const getOfficeTheme = (): string => {
    if (Office.context.officeTheme.bodyForegroundColor === "#f0f0f0") {
      return "dark";
    } else {
      return "light";
    }
  };

  /*
    The Office JavaScript Library doesn't provide a way to handle
    the theme changed event, so if a user changes themes while the
    add-in is open, the changes are not reflected in the task pane
    unless the add-in is reloaded.
  */
  const setFluentTheme = (): Theme => {
    if (getOfficeTheme() === "dark") {
      return webDarkTheme;
    } else {
      return webLightTheme;
    }
  };

  return (
    <FluentProvider theme={setFluentTheme()}>
      <div className={styles.root}>
        <div>
          <Field className={styles.inputField} size="large" label="Discount %">
            <Input value={discount} size="large" onChange={handleDiscountChange} />
          </Field>
          <Field className={styles.textAreaField} size="large" label="Offer text">
            <Textarea size="large" value={offerText} onChange={handleOfferTextChange} />
          </Field>
          <Button className={styles.button} appearance="primary" size="large" onClick={handleInsertOffer}>
            Insert Offer
          </Button>
        </div>
        <CompanyBar companyName="Blue Yonder Airlines" />
      </div>
    </FluentProvider>
  );
};

const addOfferToCRM = (offer) => {
  // The next two lines are only needed if the JSON database
  // requires each object to have a unique id property. The
  // Mockoon database does require this.
  const uuid: string = uuidv4();
  const uniqueOffer = { ...offer, id: uuid };

  fetch("http://localhost:3001/offers", {
    method: "POST",
    body: JSON.stringify(uniqueOffer),
    headers: {
      "Content-type": "application/json",
    },
  });
};

export default App;
