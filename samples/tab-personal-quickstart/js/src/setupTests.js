import "@testing-library/jest-dom";
import sinon from "sinon";

afterEach(() => {
  // Restore the default sandbox here
  sinon.restore();
});
