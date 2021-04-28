import {
  Dashboard,
  EWidgetSize,
  EChartTypes,
} from "@fluentui/react-teams";
import { random } from "../utils";

export function DashboardTab() {
  return <Dashboard widgets={dataVizWidgets} />;
}

const calloutItemsExample = [
  {
    id: "action_1",
    title: "Info",
    icon: "ExclamationCircle",
  },
  { id: "action_2", title: "Popup", icon: "Screenshare" },
  {
    id: "action_3",
    title: "Share",
    icon: "ShareGeneric",
  },
];

const linkExample = { href: "#" };

const dataVizWidgets = [
  {
    id: "w1",
    title: "Line Chart",
    desc: "Last updated Monday, April 4 at 11:15 AM (PT)",
    widgetActionGroup: calloutItemsExample,
    size: EWidgetSize.Double,
    body: [
      {
        id: "t1",
        title: "Tab 1",
        content: {
          type: "chart",
          chart: {
            title: "Line chart sample",
            type: EChartTypes.Line,
            data: {
              labels: ["Jan", "Feb", "March", "April", "May"],
              datasets: [
                {
                  label: "Tablets",
                  data: [860, 6700, 3100, 2012, 1930],
                },
                {
                  label: "Phones",
                  data: [100, 1600, 180, 3049, 3596],
                },
                {
                  label: "Laptops",
                  data: [1860, 7700, 4100, 3012, 2930],
                },
                {
                  label: "Watches",
                  data: [200, 3600, 480, 5049, 4596],
                },
                {
                  label: "TVs",
                  data: [960, 8700, 5100, 5012, 3930],
                },
                {
                  label: "Displays",
                  data: [1000, 4600, 480, 4049, 3596],
                },
              ],
            },
          },
        },
      },
      {
        id: "t2",
        title: "Tab 2",
        content: {
          type: "chart",
          chart: {
            title: "Area chart sample",
            type: EChartTypes.LineStacked,
            data: {
              labels: ["Jan", "Feb", "March", "April", "May"],
              datasets: [
                {
                  label: "Laptops",
                  data: [1860, 7700, 4100, 3012, 2930],
                },
                {
                  label: "Watches",
                  data: [200, 3600, 480, 5049, 4596],
                },
                {
                  label: "TVs",
                  data: [960, 8700, 5100, 5012, 3930],
                },
              ],
            },
          },
        },
      },
      {
        id: "t3",
        title: "Tab 3",
        content: { type: "placeholder", message: "Content #3" },
      },
    ],
    link: linkExample,
  },
  {
    id: "w2",
    title: "Doughnut chart sample",
    size: EWidgetSize.Single,
    link: linkExample,
    body: [
      {
        id: "1",
        title: "",
        content: {
          type: "chart",
          chart: {
            title: "Doughnut chart sample",
            type: EChartTypes.Doughnut,
            data: {
              labels: ["Jan", "Feb", "March", "April", "May"],
              datasets: [
                {
                  label: "Watches",
                  data: [2004, 1600, 480, 504, 1000],
                },
              ],
            },
          },
        },
      },
    ],
  },
  {
    id: "w3",
    title: "Bubble chart sample",
    size: EWidgetSize.Double,
    link: linkExample,
    body: [
      {
        id: "1",
        title: "",
        content: {
          type: "chart",
          chart: {
            title: "Bubble chart sample",
            type: EChartTypes.Bubble,
            data: {
              labels: "Africa",
              datasets: [
                {
                  label: "China",
                  data: [
                    {
                      x: 21269017,
                      y: 5.245,
                      r: 25,
                    },
                  ],
                },
                {
                  label: "Denmark",
                  data: [
                    {
                      x: 258702,
                      y: 7.526,
                      r: 10,
                    },
                  ],
                },
                {
                  label: "Germany",
                  data: [
                    {
                      x: 3979083,
                      y: 6.994,
                      r: 15,
                    },
                  ],
                },
                {
                  label: "Japan",
                  data: [
                    {
                      x: 11931877,
                      y: 5.921,
                      r: 40,
                    },
                  ],
                },
                {
                  label: "France",
                  data: [
                    {
                      x: 17269017,
                      y: 6.921,
                      r: 20,
                    },
                  ],
                },
              ],
            },
          },
        },
      },
    ],
  },
  {
    id: "w4",
    title: "Bubble chart sample",
    size: EWidgetSize.Double,
    link: linkExample,
    body: [
      {
        id: "1",
        title: "",
        content: {
          type: "chart",
          chart: {
            title: "Bubble chart sample",
            type: EChartTypes.Bubble,
            data: {
              labels: "Africa",
              datasets: [
                {
                  label: "China",
                  data: Array.from({ length: 99 }, () => ({
                    x: random(200000, 600000),
                    y: random(50, 150),
                    r: 4,
                  })),
                },
              ],
            },
          },
        },
      },
    ],
  },
  {
    id: "w5",
    title: "Pie chart",
    size: EWidgetSize.Single,
    link: linkExample,
    body: [
      {
        id: "1",
        title: "",
        content: {
          type: "chart",
          chart: {
            title: "Pie chart sample",
            type: EChartTypes.Pie,
            data: {
              labels: ["Jan", "Feb", "March", "April", "May"],
              datasets: [
                {
                  label: "Laptops",
                  data: [1860, 7700, 4100, 3012, 2930],
                },
              ],
            },
          },
        },
      },
    ],
  },
  {
    id: "w6",
    title: "Stacked area chart sample",
    size: EWidgetSize.Double,
    body: [
      {
        id: "1",
        title: "",
        content: {
          type: "chart",
          chart: {
            title: "Stacked area chart sample",
            type: EChartTypes.LineStacked,
            data: {
              labels: ["Jan", "Feb", "March", "April", "May"],
              datasets: [
                {
                  label: "Tablets",
                  data: [1860, 4700, 3100, 2012, 1930],
                },
                {
                  label: "Phones",
                  data: [1860, 1600, 180, 3049, 3596],
                },
                {
                  label: "Laptops",
                  data: [1860, 5700, 4100, 3012, 2930],
                },
              ],
            },
          },
        },
      },
    ],
    link: linkExample,
  },
  {
    id: "w7",
    title: "Gradient area chart sample",
    size: EWidgetSize.Double,
    link: linkExample,
    body: [
      {
        id: "1",
        title: "",
        content: {
          type: "chart",
          chart: {
            title: "Gradient area chart",
            type: EChartTypes.LineArea,
            data: {
              labels: ["Jan", "Feb", "March", "April", "May"],
              datasets: [
                {
                  label: "Laptops",
                  data: [1860, 7700, 4100, 3012, 2930],
                },
                {
                  label: "Watches",
                  data: [200, 3600, 480, 5049, 4596],
                },
              ],
            },
          },
        },
      },
    ],
  },
  {
    id: "w8",
    title: "Bar chart sample",
    size: EWidgetSize.Single,
    body: [
      {
        id: "Bar chart sample",
        title: "",
        content: {
          type: "chart",
          chart: {
            title: "Bar chart sample",
            type: EChartTypes.Bar,
            data: {
              labels: ["Jan", "Feb", "March", "April", "May"],
              datasets: [
                {
                  label: "Watches",
                  data: [2300, 3600, 4800, 5049, 4596],
                },
              ],
            },
          },
        },
      },
    ],
    link: linkExample,
  },
  {
    id: "w9",
    title: "Stacked bar chart sample",
    size: EWidgetSize.Single,
    body: [
      {
        id: "Stacked bar chart sample",
        title: "",
        content: {
          type: "chart",
          chart: {
            title: "Stacked bar chart sample",
            type: EChartTypes.BarStacked,
            data: {
              labels: ["Jan", "Feb", "March", "April", "May"],
              datasets: [
                {
                  label: "Laptops",
                  data: [1860, 7700, 4100, 3012, 2930],
                },
                {
                  label: "Watches",
                  data: [1200, 3600, 2480, 5049, 4596],
                },
              ],
            },
          },
        },
      },
    ],
    link: linkExample,
  },
  {
    id: "w10",
    title: "Horizontal bar chart sample",
    size: EWidgetSize.Single,
    body: [
      {
        id: "Horizontal bar chart sample",
        title: "",
        content: {
          type: "chart",
          chart: {
            title: "Horizontal bar chart sample",
            type: EChartTypes.BarHorizontal,
            data: {
              labels: ["Jan", "Feb", "March", "April", "May"],
              datasets: [
                {
                  label: "Watches",
                  data: [200, 3600, 480, 5049, 4596],
                },
              ],
            },
          },
        },
      },
    ],
    link: linkExample,
  },
  {
    id: "w11",
    title: "Grouped bar chart sample",
    size: EWidgetSize.Double,
    body: [
      {
        id: "1",
        title: "",
        content: {
          type: "chart",
          chart: {
            title: "Grouped bar chart sample",
            type: EChartTypes.Bar,
            data: {
              labels: ["Jan", "Feb", "March", "April", "May"],
              datasets: [
                {
                  label: "Tablets",
                  data: [4860, 6700, 3100, 2012, 1930],
                },
                {
                  label: "Phones",
                  data: [4100, 1600, 3180, 3049, 3596],
                },
                {
                  label: "Laptops",
                  data: [1860, 7700, 4100, 3012, 2930],
                },
                {
                  label: "Watches",
                  data: [1200, 3600, 2480, 5049, 4596],
                },
              ],
            },
          },
        },
      },
    ],
    link: linkExample,
  },
  {
    id: "w12",
    title: "Horizontal stacked bar chart sample",
    size: EWidgetSize.Double,
    body: [
      {
        id: "Horizontal stacked bar chart sample",
        title: "",
        content: {
          type: "chart",
          chart: {
            title: "Horizontal stacked bar chart sample",
            type: EChartTypes.BarHorizontalStacked,
            data: {
              labels: ["Jan", "Feb", "March", "April", "May"],
              datasets: [
                {
                  label: "Laptops",
                  data: [1860, 7700, 4100, 3012, 2930],
                },
                {
                  label: "Watches",
                  data: [1200, 3600, 2480, 5049, 4596],
                },
              ],
            },
          },
        },
      },
    ],
    link: linkExample,
  },
  {
    id: "w13",
    title: "Error chart state",
    size: EWidgetSize.Single,
    body: [
      {
        id: "1",
        title: "",
        content: {
          type: "chart",
          chart: { title: "Chart error state", type: EChartTypes.LineStacked },
        },
      },
    ],
    link: linkExample,
  },
  {
    id: "w14",
    title: "No data chart state",
    size: EWidgetSize.Single,
    body: [
      {
        id: "1",
        title: "",
        content: {
          type: "chart",
          chart: {
            title: "Chart no data state",
            type: EChartTypes.LineStacked,
            data: {
              labels: [],
              datasets: [],
            },
          },
        },
      },
    ],
    link: linkExample,
  },
  {
    id: "w15",
    title: "Card 6",
    size: EWidgetSize.Single,
    link: linkExample,
  },
];
