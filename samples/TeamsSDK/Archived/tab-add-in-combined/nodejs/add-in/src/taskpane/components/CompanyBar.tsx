import * as React from "react";
import { tokens, makeStyles } from "@fluentui/react-components";

const useStyles = makeStyles({
  companyBar: {
    height: "10vh",
    backgroundColor: tokens.colorBrandBackground,
    color: tokens.colorNeutralForegroundOnBrand,
    paddingBottom: "10px",
    paddingLeft: "10px",
    paddingRight: "10px",
    paddingTop: "10px",
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    // Set font size to 10% of viewport width so the text
    // shrinks and grows as the task pane width changes.
    // This ensures that it doesn't wrap unless the viewport
    // is extremely narrow. The formula is supposed to ensure
    // that the font is never more than 50px, so it doesn't
    // become huge on a Teams tab.
    fontSize: "calc(30px + 20 * ((100vw - 200px) / 3800));",
  },
});

interface CompanyBarProps {
  companyName: string;
}

const CompanyBar: React.FC<CompanyBarProps> = ({ companyName }) => {
  const styles = useStyles();
  return <div className={styles.companyBar}>{companyName}</div>;
};

export default CompanyBar;
