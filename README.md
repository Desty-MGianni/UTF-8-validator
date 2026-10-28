# UTF-8 CLI Validator

A lightweight, high-performance command-line tool written in C# designed to strictly validate files against the UTF-8 specification ([RFC 3629](https://ietf.org)).

Inspired by Dave Eddy's implementation from the YouTube channel _You Suck At Programming_, this repository features enhanced logic routines to securely process single-byte ASCII, multi-byte character constraints, and edge-case byte sequences.

Here is a link to [the YouTube video](https://www.youtube.com/watch?v=7taRssuPB8E)

## Features

- **Strict Specification Matching:** Correctly differentiates between ASCII (`0x00`-`0x7F`), continuation bytes (`0x80`-`0xBF`), and structural leading bytes.
- **Forbidden Value Detection:** Rejects invalid standalone bytes like `0xC0`, `0xC1`, or any value above `0xF4` that exceeds the `U+10FFFF` Unicode limit.
- **Batch Processing:** Pass multiple file paths at once to test batches of files simultaneously.
- **Colorized CLI Output:** Instant visual cues for testing workflows (`[error]` prints in bright red).

## Installation

### Prerequisites

- [.NET 8.0 SDK](https://microsoft.com) or later.

### Clone the Repository

```bash
git clone https://github.com
cd utf8-validator
```

## Usage

Run the validator by passing one or multiple file paths as arguments to your compiled binary or via `dotnet run`:

```bash
dotnet run -- path/to/file1.txt path/to/file2.bin
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

## Roadmap / TODO

- [ ] Implement robust **Overlong Encoding** traps (e.g., detecting `0xE0` followed by bytes lower than `0xA0`).
- [ ] Add explicit UTF-16 Surrogate Pair filtering (blocking `0xED` sub-ranges).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
