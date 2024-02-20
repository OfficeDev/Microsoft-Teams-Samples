import * as React from "react";
import { Card, CardHeader, CardFooter, Text, Link, Body1, Caption1, Tab, TabList, makeStyles, shorthands, Menu, MenuTrigger, MenuButton, MenuPopover, MenuList, MenuItem} from "@fluentui/react-components";
import { ErrorCircle20Regular, ShareScreenPerson24Regular, Share24Filled, MoreHorizontal20Regular } from "@fluentui/react-icons";
import type {
  SelectTabData,
  SelectTabEvent,
  TabValue,
} from "@fluentui/react-components";

interface ContentProps {
  value: string; // Define the type of the 'value' prop
}

const linkExample = { href: "#" };

const useStyles = makeStyles({
  root: {
    alignItems: "flex-start",
    display: "flex",
    flexDirection: "column",
    justifyContent: "flex-start",
    rowGap: "20px",
  },
  panels: {
    width: "100%"
  },
});

const Content: React.FC<ContentProps> = React.memo(({ value }) => {
  const boxStyle = {
    width: 'auto',
    height: '200px',
    border: '2px dotted #727272',
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
  };

  return (
    <div style={boxStyle}>
      <span>{value}</span>
    </div>
  );
});

export function DashboardTab() {
  const [selectedValue, setSelectedValue] = React.useState<TabValue>("Tab1");

  const onTabSelect = (event: SelectTabEvent, data: SelectTabData) => {
    setSelectedValue(data.value);
  };
  const styles = useStyles();
  return (
    <div style={{ display: "flex", flexWrap: "wrap" }}>
      {defaultWidgets.map((widget, index) => (
        <Card
          key={index}
          style={{
            width: widget.size,
            margin: "10px",
            padding: "15px",
            border: "1px solid #ccc",
            borderRadius: "8px",
          }}
        >
          <CardHeader
            header={
              <Body1>
                <b>
                  {widget.title}
                </b>
              </Body1>
            }
            description={<Caption1>{widget.desc}</Caption1>}
            action={
              <Menu>
                <MenuTrigger disableButtonEnhancement>
                  <MenuButton appearance="transparent" icon={<MoreHorizontal20Regular />}>
                  </MenuButton>
                </MenuTrigger>
                <MenuPopover>
                  <MenuList>
                    <MenuItem icon={<ErrorCircle20Regular />}>icon</MenuItem>
                    <MenuItem icon={<ShareScreenPerson24Regular />}>Popup</MenuItem>
                    <MenuItem icon={<Share24Filled />}>Share</MenuItem>
                  </MenuList>
                </MenuPopover>
              </Menu>
            }
          />
          <div className={styles.root}>
            <TabList selectedValue={selectedValue} onTabSelect={onTabSelect}>
              {widget.body?.map((tab, tabIndex) => (
                <>
                  {tab.content}
                </>
              ))}
            </TabList>
            <div className={styles.panels}>
              {widget.body && selectedValue === "Tab1" && <Content value="Content #1" />}
              {widget.body && selectedValue === "Tab2" && <Content value="Content #2" />}
              {widget.body && selectedValue === "Tab3" && <Content value="Content #3" />}
              {!widget.body && <Content value="" />}
            </div>
          </div>
          <CardFooter>
            <Link href={widget.link?.href}>
              View more
            </Link>
          </CardFooter>
        </Card>
      ))}
    </div>
  );
}


const defaultWidgets = [
  {
    title: "Card 1",
    desc: "Last updated Monday, April 4 at 11:15 AM (PT)",
    size: "75%",
    body: [
      {
        id: "t1",
        title: "Tab 1",
        content: (
          <Tab id="Tab1" value="Tab1">
            Tab 1
          </Tab>
        ),
      },
      {
        id: "t2",
        title: "Tab 2",
        content: (
          <Tab id="Tab2" value="Tab2">
            Tab 2
          </Tab>
        ),
      },
      {
        id: "t3",
        title: "Tab 3",
        content: (
          <Tab id="Tab3" value="Tab3">
            Tab 3
          </Tab>
        ),
      },
    ],
    link: linkExample,
  },
  {
    title: "Card 2",
    size: "20%",
    link: linkExample,
  },
  {
    title: "Card 3",
    size: "55%",
    link: linkExample,
  },
  {
    title: "Card 4",
    size: "20%",
    link: linkExample,
  },
  {
    title: "Card 5",
    size: "20%",
    link: linkExample,
  },
  {
    title: "Card 6",
    size: "100%",
    link: linkExample,
  },
];
