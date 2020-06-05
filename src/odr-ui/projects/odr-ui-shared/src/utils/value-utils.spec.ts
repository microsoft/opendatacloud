// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { isEmptyValue } from "./value-utils";

describe("isEmptyValue", () => {
  it("should return true for null", () => {
    expect(isEmptyValue(null)).toBe(true);
  });
  it("should return true for undefined", () => {
    expect(isEmptyValue(undefined)).toBe(true);
  });
  it("should return true for empty string", () => {
    expect(isEmptyValue("")).toBe(true);
  });
  it("should return true for whitespace string", () => {
    expect(isEmptyValue("  \t  \n  \r   ")).toBe(true);
  });
  it("should return false for non-whitespace string", () => {
    expect(isEmptyValue("abc")).toBe(false);
  });
  it("should return false for non-string", () => {
    expect(isEmptyValue(123)).toBe(false);
  });
});
