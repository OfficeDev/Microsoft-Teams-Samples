// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import TabConfig from "./TabConfig";

const TabWrapper = () => {
  const gray = "Tab.js says: 'You chose Gray!'";
  const red = "Tab.js says: 'You chose Red!'";

  return <TabConfig gray={gray} red={red} />;
};

export default TabWrapper;
