import React from "react";
import { Dashboard } from "@fluentui/react-teams";
import {
  IWidget,
  WidgetSize,
} from "@fluentui/react-teams/lib/components/Dashboard/DashboardWidget";
import {
  ExclamationCircleIcon,
  ScreenshareIcon,
  ShareGenericIcon,
  Flex,
  Text,
} from "@fluentui/react-northstar";

export function DashboardTab() {
  return <Dashboard widgets={defaultWidgets} />;
}

const calloutItemsExample = [
  {
    id: "action_1",
    title: "Info",
    icon: <ExclamationCircleIcon />,
  },
  { id: "action_2", title: "Popup", icon: <ScreenshareIcon /> },
  {
    id: "action_3",
    title: "Share",
    icon: <ShareGenericIcon />,
  },
];

const linkExample = { href: "#" };

const defaultWidgets: IWidget[] = [
  {
    title: "Card 1",
    desc: "Last updated Monday, April 4 at 11:15 AM (PT)",
    widgetActionGroup: calloutItemsExample,
    size: WidgetSize.Triple,
    body: [
      {
        id: "t1",
        title: "Tab 1",
        content: (
          <Flex
            vAlign="center"
            hAlign="center"
            styles={{ height: "100%", border: "1px dashed rgb(179, 176, 173)" }}
          >
            <Text size="large" weight="semibold">
              Content #1
            </Text>
          </Flex>
        ),
      },
      {
        id: "t2",
        title: "Tab 2",
        content: (
          <Flex
            vAlign="center"
            hAlign="center"
            styles={{ height: "100%", border: "1px dashed rgb(179, 176, 173)" }}
          >
            <Text size="large" weight="semibold">
              Content #2
            </Text>
          </Flex>
        ),
      },
      {
        id: "t3",
        title: "Tab 3",
        content: (
          <Flex
            vAlign="center"
            hAlign="center"
            styles={{ height: "100%", border: "1px dashed rgb(179, 176, 173)" }}
          >
            <Text size="large" weight="semibold">
              Content #3
            </Text>
          </Flex>
        ),
      },
    ],
    link: linkExample,
  },
  {
    title: "Card 2",
    size: WidgetSize.Single,
    link: linkExample,
  },
  {
    title: "Card 3",
    size: WidgetSize.Double,
    link: linkExample,
  },
  {
    title: "Card 4",
    size: WidgetSize.Single,
    link: linkExample,
  },
  {
    title: "Card 5",
    size: WidgetSize.Single,
    link: linkExample,
  },
  {
    title: "Card 6",
    size: WidgetSize.Single,
    link: linkExample,
  },
];
