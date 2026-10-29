# UTF-8 CLI Validator

A lightweight, high-performance command-line tool written in C# designed to strictly validate files against the UTF-8 specification ([RFC 3629](https://www.rfc-editor.org/info/rfc3629/)).

Inspired by Dave Eddy's implementation from the YouTube channel _You Suck At Programming_, this repository features enhanced logic routines to securely process single-byte ASCII, multi-byte character constraints, and edge-case byte sequences.

Here is a link to [the YouTube video](https://www.youtube.com/watch?v=7taRssuPB8E)

## Features

- **Strict Specification Matching:** Correctly differentiates between ASCII (`0x00`-`0x7F`), continuation bytes (`0x80`-`0xBF`), and structural leading bytes.
- **Forbidden Value Detection:** Rejects invalid standalone bytes like `0xC0`, `0xC1`, or any value above `0xF4` that exceeds the `U+10FFFF` Unicode limit.
- **Batch Processing:** Pass multiple file paths at once to test batches of files simultaneously.
- **Colorized CLI Output:** Instant visual cues for testing workflows (`[error]` prints in bright red).

## Installation

### Prerequisites

- .NET 8.0 SDK or later.

### Clone the Repository

```bash
git clone https://github.com/Desty-MGianni/UTF-8-validator
cd utf8-validator
```

## Usage

Run the validator by passing one or multiple file paths as arguments to your compiled binary or via `dotnet run`:

```bash
dotnet run file1 file2
```

### Example Outputs

**When all files are valid:**

```text
All tests passed successfully!
```

**When corruption is found:**

```text
[error] [bad_file.txt] The first byte of the UTF-8 character is invalid: 255 -> FF
1/2 tests has been successful
```

## Technical Architecture

The validator sequentially steps through a file stream and routes evaluation via localized byte matching:

- **Single-Byte Boundaries:** `0x00` to `0x7F`.
- **Multi-Byte Boundaries:** Correctly counts expected trailing bytes (`1` for `0xC2`-`0xDF`, `2` for `0xE0`-`0xEF`, `3` for `0xF0`-`0xF4`).
- **Truncation Safeguards:** Detects unexpected EOF mid-sequence.

## Limitations

- The program uses a simple filename string instead of an actual path. Which mean that the test files have to be inside the working directory.
- The program only read files and can't accepts folders or any raw bytes parsed into it.

## Roadmap / TODO

- [x] Implement robust **Overlong Encoding** traps (e.g., detecting `0xE0` followed by bytes lower than `0xA0`).
- [ ] Add explicit UTF-16 Surrogate Pair filtering (blocking `0xED` sub-ranges)
- [ ] Implement **set of arguments** to notify the program the type of media wegive it (`-d`, `-f` or `-t`).

## License

This project is licensed under the MIT License.

```text
MIT License

Copyright (c) 2026 Gianni Mucci

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
