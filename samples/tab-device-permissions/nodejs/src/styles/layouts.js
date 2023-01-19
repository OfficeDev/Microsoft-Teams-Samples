/*!
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import { makeStyles } from "@fluentui/react-components";

export const getFlexRowStyles = makeStyles({
  root: {
    display: "flex",
    minHeight: "0px",
  },
  fill: {
    width: "100%",
    height: "100%",
  },
  smallGap: {
    "> :not(:last-child)": {
      marginRight: "0.5rem",
    },
  },
  spaceBetween: {
    justifyContent: "space-between",
  },
  hAlignStart: {
    justifyContent: "start",
  },
  hAlignCenter: {
    justifyContent: "center",
  },
  hAlignEnd: {
    justifyContent: "end",
  },
  vAlignStart: {
    alignItems: "start",
  },
  vAlignCenter: {
    alignItems: "center",
  },
  vAlignEnd: {
    alignItems: "end",
  },
});

export const getFlexColumnStyles = makeStyles({
  root: {
    display: "flex",
    flexDirection: "column",
    minHeight: "0px",
  },
  fill: {
    width: "100%",
    height: "100%",
  },
  smallGap: {
    "> :not(:last-child)": {
      marginBottom: "0.5rem",
    },
  },
  spaceBetween: {
    justifyContent: "space-between",
  },
  hAlignStart: {
    alignItems: "start",
  },
  hAlignCenter: {
    alignItems: "center",
  },
  hAlignEnd: {
    alignItems: "end",
  },
  vAlignStart: {
    justifyContent: "start",
  },
  vAlignCenter: {
    justifyContent: "center",
  },
  vAlignEnd: {
    justifyContent: "end",
  },
  scroll: {
    overflowY: "auto",
    msOverflowStyle: "auto",
    maxHeight: "100vh",
  },
});

export const getFlexItemStyles = makeStyles({
  grow: {
    flexGrow: 1,
  },
  noShrink: {
    flexShrink: 0,
  },
});