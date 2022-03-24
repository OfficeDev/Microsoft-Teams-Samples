import range from "lodash/range";
import uniqueId from "lodash/uniqueId";
import fakerEN from "faker/locale/en_US";
import fakerFA from "faker/locale/fa";
// Form component import
import { Form, EInlineInputType, EInputBlockType, EInputWidth, ISection } from "@fluentui/react-teams";

export function FormTab() {
  return <Form {...formConfig} />;
}

const fake = (template: string) => {
  return { "en-US": fakerEN.fake(template), fa: fakerFA.fake(template) };
};

const formConfig = {
  submit: {
    en: "Okay",
    fa: "تایید",
  },
  cancel: {
    en: "Cancel",
    fa: "لغو",
  },
  headerSection: {
    title: fake("{{company.catchPhrase}}"),
    preface: fake("{{lorem.sentences}}"),
  },
  sections: [
    {
      title: fake("{{company.catchPhrase}}"),
      preface: fake("{{lorem.sentence}}"),
      inputBlocks: [
        {
          type: EInputBlockType.inlineInputs,
          fields: range(2).map((_) => ({
            type: EInlineInputType.text,
            title: fake("{{commerce.productAdjective}} {{commerce.product}}"),
            width: EInputWidth.split,
            inputId: uniqueId("f"),
            placeholder: fake("{{commerce.productMaterial}}"),
          })),
        },
        {
          type: EInputBlockType.inlineInputs,
          fields: [
            {
              type: EInlineInputType.dropdown,
              title: fake("{{commerce.productAdjective}} {{commerce.product}}"),
              width: EInputWidth.split,
              inputId: uniqueId("f"),
              multiple: false,
              options: range(2 + Math.random() * 5).map(() => ({
                title: fake("{{commerce.productMaterial}}"),
                value: uniqueId("option__"),
              })),
            },
            {
              type: EInlineInputType.text,
              title: fake("{{commerce.product}}"),
              width: EInputWidth.split,
              inputId: uniqueId("f"),
              ...(Math.random() > 0.5
                ? { placeholder: fake("{{commerce.productMaterial}}") }
                : {}),
            },
            {
              type: EInlineInputType.text,
              title: fake("{{commerce.productAdjective}} {{commerce.product}}"),
              width: EInputWidth.split,
              inputId: uniqueId("f"),
              ...(Math.random() > 0.5
                ? { placeholder: fake("{{commerce.productMaterial}}") }
                : {}),
            },
          ],
        },
        {
          type: EInputBlockType.checkboxes,
          title: fake("{{commerce.productAdjective}} {{commerce.product}}"),
          inputId: uniqueId("f"),
          options: range(2 + Math.random() * 5).map(() => ({
            title: fake("{{commerce.productMaterial}}"),
            value: uniqueId("option__"),
          })),
        },
        {
          type: EInputBlockType.inlineInputs,
          fields: range(1).map((_) => ({
            type: "text" as "text",
            title: fake("{{commerce.productAdjective}} {{commerce.product}}"),
            width: EInputWidth.full,
            inputId: uniqueId("f"),
            ...(Math.random() > 0.5
              ? { placeholder: fake("{{commerce.productMaterial}}") }
              : {}),
          })),
        },
        {
          type: EInputBlockType.radioButtons,
          title: fake("{{commerce.productAdjective}} {{commerce.product}}"),
          inputId: uniqueId("f"),
          options: range(2 + Math.random() * 5).map(() => ({
            title: fake("{{commerce.productMaterial}}"),
            value: uniqueId("option__"),
          })),
        },
        {
          type: EInputBlockType.dropdown,
          title: fake("{{commerce.productAdjective}} {{commerce.product}}"),
          inputId: uniqueId("f"),
          multiple: true,
          options: range(2 + Math.random() * 5).map(() => ({
            title: fake("{{commerce.productMaterial}}"),
            value: uniqueId("option__"),
          })),
        },
      ],
    },
  ] as ISection[],
};