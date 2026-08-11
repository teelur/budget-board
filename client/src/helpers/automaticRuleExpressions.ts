type AmountExpressionToken =
  | { kind: "number"; value: string }
  | { kind: "amount" }
  | { kind: "operator"; value: "+" | "-" | "*" | "/" }
  | { kind: "leftParen" }
  | { kind: "rightParen" };

const numericLiteralPattern = /^\d+(?:\.\d*)?$/;

const tokenize = (expression: string): AmountExpressionToken[] | null => {
  const tokens: AmountExpressionToken[] = [];
  let index = 0;

  while (index < expression.length) {
    const character = expression[index];

    if (character === undefined) {
      return null;
    }

    if (/\s/.test(character)) {
      index += 1;
      continue;
    }

    if (/\d|\./.test(character)) {
      const start = index;

      if (character === "." && !/\d/.test(expression[index + 1] ?? "")) {
        return null;
      }

      index += 1;
      while (/\d/.test(expression[index] ?? "")) {
        index += 1;
      }

      if (expression[index] === ".") {
        index += 1;
        while (/\d/.test(expression[index] ?? "")) {
          index += 1;
        }
      }

      if (expression[index] === "e" || expression[index] === "E") {
        index += 1;
        if (expression[index] === "+" || expression[index] === "-") {
          index += 1;
        }

        const exponentStart = index;
        while (/\d/.test(expression[index] ?? "")) {
          index += 1;
        }

        if (exponentStart === index) {
          return null;
        }
      }

      const value = expression.slice(start, index);
      if (!numericLiteralPattern.test(value)) {
        return null;
      }

      tokens.push({ kind: "number", value });
      continue;
    }

    if (expression.slice(index, index + "amount".length).toLowerCase() === "amount") {
      tokens.push({ kind: "amount" });
      index += "amount".length;
      continue;
    }

    if (character === "+" || character === "-" || character === "*") {
      tokens.push({ kind: "operator", value: character });
      index += 1;
      continue;
    }

    if (character === "/") {
      tokens.push({ kind: "operator", value: "/" });
      index += 1;
      continue;
    }

    if (character === "(") {
      tokens.push({ kind: "leftParen" });
      index += 1;
      continue;
    }

    if (character === ")") {
      tokens.push({ kind: "rightParen" });
      index += 1;
      continue;
    }

    return null;
  }

  return tokens;
};

export const isNumericLiteral = (value: string): boolean =>
  numericLiteralPattern.test(value.trim());

export const isValidAmountExpression = (expression: string): boolean => {
  const tokens = tokenize(expression);
  if (!tokens || tokens.length === 0) {
    return false;
  }

  let position = 0;

  const parseExpression = (): boolean => {
    if (!parseMultiplication()) {
      return false;
    }

    while (true) {
      const token = tokens[position];
      if (
        token?.kind !== "operator" ||
        (token.value !== "+" && token.value !== "-")
      ) {
        break;
      }

      position += 1;
      if (!parseMultiplication()) {
        return false;
      }
    }

    return true;
  };

  const parseMultiplication = (): boolean => {
    if (!parseUnary()) {
      return false;
    }

    while (true) {
      const token = tokens[position];
      if (
        token?.kind !== "operator" ||
        (token.value !== "*" && token.value !== "/")
      ) {
        break;
      }

      position += 1;
      if (!parseUnary()) {
        return false;
      }
    }

    return true;
  };

  const parseUnary = (): boolean => {
    const token = tokens[position];
    if (
      token?.kind === "operator" &&
      (token.value === "+" || token.value === "-")
    ) {
      position += 1;
      return parseUnary();
    }

    return parsePrimary();
  };

  const parsePrimary = (): boolean => {
    const token = tokens[position];

    if (token?.kind === "number" || token?.kind === "amount") {
      position += 1;
      return true;
    }

    if (token?.kind !== "leftParen") {
      return false;
    }

    position += 1;
    if (!parseExpression() || tokens[position]?.kind !== "rightParen") {
      return false;
    }

    position += 1;
    return true;
  };

  return parseExpression() && position === tokens.length;
};
