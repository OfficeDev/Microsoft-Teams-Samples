import {
  Grid,
  Box,
  Flex,
  Text,
  Image,
  SiteVariablesPrepared,
} from "@fluentui/react-northstar";
import platformUiImage from "../../assets/images/platform-ui.jpg";
import teamsTemplatesImage from "../../assets/images/teams-templates.jpg";
import teamsAppsImage from "../../assets/images/teams-apps.jpg";
import visualStudioImage from "../../assets/images/visual-studio.jpg";

export function WelcomeTab() {
  return (
    <Box
      style={{
        margin: "1.6rem 1.25rem",
      }}
    >
      <Text size="large" weight="bold" as="h1">
        Welcome to our Teams Sample App
      </Text>
      <Text
        as="p"
        variables={({ colorScheme }: SiteVariablesPrepared) => {
          console.log(colorScheme.default.foreground2);
          return {
            border: "1px solid red",
            backgroundColor: "red",
          };
        }}
        styles={{ marginBottom: "3rem", opacity: ".65", maxWidth: "50rem" }}
      >
        The Teams Sample App can help you better understand how your app should
        look and behave in Teams depending on the scenario. Select a UI template
        above (for example, Forms) to get started. To learn more, see the
        following resources.
      </Text>
      <Text size="medium" weight="bold" as="h2">
        Resources
      </Text>
      <Grid
        styles={{
          gridGap: "2rem",
          gridTemplate:
            "repeat(auto-fill, 20rem) / repeat(auto-fill, minmax(16rem, 1fr))",
          gridAutoFlow: "dense",
          gridAutoRows: "26rem",
          padding: "0 1rem 1.25rem",
          margin: "1.5rem -1rem",
          minWidth: "20rem",
          "@media (max-width: 986px)": {
            gridTemplate:
              "repeat(auto-fill, 25rem) / repeat(auto-fill, minmax(15.75rem, 1fr))",
          },
        }}
      >
        {resources.map((resource: IResourceCard, key: number) => (
          <ResourceCard key={key} {...resource} />
        ))}
      </Grid>
    </Box>
  );
}

interface IResourceCardLink {
  label: string;
  href: string;
}

interface IResourceCard {
  imageUrl: string;
  title: string;
  desc: string;
  links: IResourceCardLink[];
}
const ResourceCard = ({ imageUrl, title, desc, links }: IResourceCard) => {
  return (
    <Flex column>
      <Image
        src={imageUrl}
        alt={title}
        styles={{
          borderRadius: "4px",
          boxShadow:
            "0px 3px 6px rgba(0, 0, 0, 0.1), 0px 1px 5px rgba(0, 0, 0, 0.06)",
        }}
        fluid
      />
      <Text
        size="medium"
        weight="semibold"
        as="h3"
        styles={{ margin: "2rem 0 .5rem" }}
      >
        {title}
      </Text>
      <Text
        as="p"
        styles={{ margin: ".5rem 0 2rem", flexGrow: 1, opacity: ".65" }}
      >
        {desc}
      </Text>
      {links.map((link: IResourceCardLink, key: number) => (
        <Text
          key={key}
          as="a"
          href={link.href}
          target="_blank"
          content={link.label}
          size="small"
          color="brand"
          styles={{ textDecoration: "none", marginBottom: ".5rem" }}
        />
      ))}
    </Flex>
  );
};

const resources: IResourceCard[] = [
  {
    imageUrl: platformUiImage,
    title: "Design with the Teams UI Kit",
    desc: `The Microsoft Teams UI Kit includes UI components, templates, best practices, and other comprehensive resources to help design your Teams app.`,
    links: [
      {
        label: "Get the UI Kit",
        href: "https://www.figma.com/community/file/916836509871353159",
      },
    ],
  },
  {
    imageUrl: teamsTemplatesImage,
    title: "Use Teams app templates",
    desc: `Browse our collection of production-ready, open-source apps that you can customize or deploy right away to Teams.`,
    links: [
      {
        label: "See the templates",
        href:
          "https://docs.microsoft.com/en-us/microsoftteams/platform/samples/app-templates",
      },
    ],
  },
  {
    imageUrl: teamsAppsImage,
    title: "Learn Teams development",
    desc: `Are you new to Teams development? To familiarize yourself, quickly build a “Hello, World!” app or read all about how Teams apps work.`,
    links: [
      {
        label: "See more",
        href:
          "https://docs.microsoft.com/en-us/microsoftteams/platform/overview",
      },
    ],
  },
  {
    imageUrl: visualStudioImage,
    title: "Build with the Teams Toolkit",
    desc:
      "The Microsoft Teams Toolkit extension is the fastest way to build, test, and deploy your app to Teams.",
    links: [
      {
        label: "Get the Visual Studio Code extension",
        href:
          "https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.vsteamstemplate&ssr=false",
      },
      {
        label: "Get the Visual Studio extension",
        href:
          "https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.vsteamstemplate&ssr=false",
      },
    ],
  },
];
