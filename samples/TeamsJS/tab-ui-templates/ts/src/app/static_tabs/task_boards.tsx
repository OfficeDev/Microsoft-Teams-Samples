import { Board } from "@fluentui/react-teams";
import {
  IBoardItem,
  IBoardItemCardLayout,
} from "@fluentui/react-teams/lib/components/Board/Board";
import { TUsers } from "@fluentui/react-teams/lib/types/types";
// TODO: Avoid faker
import { fake, image } from "faker";
import { people } from "@uifabric/example-data";

export function BoardsTab() {
  return <Board {...boardContent} boardItemCardLayout={boardItemCardLayout} />;
}

const range = (start: number, end: number): number[] => {
  const out = []
  while (start < end) out.push(start++)
  return out
}

const shuffle = <T,>(xs: T[]): T[] => {
  const shuffled = [...xs];
  for (let i = shuffled.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
  }
  return xs;
};

const usersRange = range(1, 25)

const users = () =>
  shuffle(
    usersRange.filter(() => Math.random() > 0.67).map((idx) => `u${idx}`)
  );

const boardContent = {
  users: usersRange.reduce((acc: TUsers, i: number) => {
    acc[`u${i}`] = {
      name: fake("{{name.findName}}"),
      ...(Math.random() > 0.33 ? { image: people[i].imageUrl } : {}),
    };
    return acc;
  }, {}),
  lanes: {
    l1: {
      title: fake("{{commerce.department}}"),
    },
    l2: {
      title: fake("{{commerce.department}}"),
    },
    l3: {
      title: fake("{{commerce.department}}"),
    },
    l4: {
      title: fake("{{commerce.department}}"),
    },
    l5: {
      title: fake("{{commerce.department}}"),
    },
  },
  items: range(2, 6).reduce(
    (
      acc: { ii: number; items: { [itemKey: string]: IBoardItem } },
      li: number
    ) => {
      for (let lo = 0; lo < (li - 1) * 2; lo++) {
        acc.items[`t${acc.ii + lo}`] = {
          lane: `l${li}`,
          order: lo,
          title: fake(
            "{{commerce.productAdjective}} {{commerce.productMaterial}} {{commerce.product}}"
          ),
          ...(Math.random() > 0.33
            ? { subtitle: fake("{{company.catchPhrase}}") }
            : {}),
          ...(Math.random() > 0.33 ? { body: fake("{{lorem.sentence}}") } : {}),
          ...(Math.random() > 0.33 ? { preview: image.image() } : {}),
          ...(Math.random() > 0.33 ? { users: users() } : {}),
          ...(Math.random() > 0.5
            ? {
                badges: {
                  attachments: Math.max(1, Math.floor(999 * Math.random())),
                },
              }
            : {}),
        };
      }
      acc.ii += (li - 1) * 2;
      return acc;
    },
    { ii: 0, items: {} }
  ).items,
};

const boardItemCardLayout: IBoardItemCardLayout = {
  previewPosition: "top",
  overflowPosition: "footer",
};
