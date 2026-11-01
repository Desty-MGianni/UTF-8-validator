# UTF-8 CLI Validator

A lightweight, high-performance command-line tool written in C# designed to strictly validate files against the UTF-8 specification ([RFC 3629](https://www.rfc-editor.org/info/rfc3629/)).

Inspired by Dave Eddy's implementation from the YouTube channel _You Suck At Programming_, this repository features enhanced logic routines to securely process single-byte ASCII, multi-byte character constraints, and edge-case byte sequences.

Here is a link to [the YouTube video](https://www.youtube.com/watch?v=7taRssuPB8E)

## Features

- **Strict Specification Matching:** Correctly differentiates between ASCII (`0x00`-`0x7F`), continuation bytes (`0x80`-`0xBF`), and structural leading bytes.
- **Forbidden Value Detection:** Rejects invalid standalone bytes like `0xC0`, `0xC1`, or any value above `0xF4` that exceeds the `U+10FFFF` Unicode limit.
- **Overlong and Surrogate Pairs Verification:** Automatically calculates expected continuation bytes and strictly enforces non-overlong boundaries and UTF-16 surrogate blocks.
  **Flexible Input Routing:** Seamlessly processes directories (`-d`), file lists (`-f`), textual hex escape representations (`-t`), or raw piped binary streams via standard input.
- **Flexible Output Routing** Different output layers via `--debug` and `--show` global flags.
- **Smart Stdin Pipe Detection:** Automatically senses if data is being piped (`|`) or redirected (`<`), adjusting its interface without freezing or requiring tedious arguments.
- **Batch Processing:** Pass multiple file paths at once to test batches of files simultaneously.
- **Colorized CLI Output:** Instant visual cues for testing workflows (`[error]` prints in bright red).

## Installation

### Prerequisites

- .NET 8.0 SDK or later.

### Clone and Compile

```bash
git clone https://github.com/Desty-MGianni/UTF-8-validator
cd utf8-validator
dotnet publish -c Release
```

## Usage

The tool adapts dynamically to how you invoke it.
Both global flags (`--global` and `--show`) can be added anywhere inside the argument list.

### 1. File & Directory Scanning (Batch Mode)

```bash
# Scan specific files
./utf8-validator -f file1.txt path/to/file2.log --debug

# Scan all files within directories
./utf8-validator -d path/to/project /var/log/

# With a global flag in the middle
./utf8-validator -t "\x23\x33" --show "\x0a\x99"
```

### 2. Hex/Text Suffix Generation Mode

```bash
# Manually parse hex formatting directly into real byte values (printf syntax)
./utf8-validator -t --debug "\xAC\x41\x42"
```

When no argumens are given after the flag, the program prompt the user via stdin. To escape from it

### 3. Piped Stream Mode (Native Unix Architecture)

No flags are required when piping data. The application instantly switches to streaming mode upon detecting input redirection:

```bash
# Pipe raw binary bytes from another tool
printf "\xFF" | ./utf8-validator

# Redirect a binary payload directly into the standard input
./utf8-validator < firmware.bin
```

## Technical Architecture

The validator sequentially steps through a file stream and routes evaluation via localized byte matching:

- **Single-Byte Boundaries:** `0x00` to `0x7F`.
- **Multi-Byte Boundaries:** Correctly counts expected trailing bytes (`1` for `0xC2`-`0xDF`, `2` for `0xE0`-`0xEF`, `3` for `0xF0`-`0xF4`).
- **Overlong Detection:** Check the value of the 2nd byte depending of the number of continous bytes needed for the character.

| Continuous Bytes | Error condition   |
| ---------------- | ----------------- |
| 1                | 2nd byte < `0xA0` |
| 2                | 2nd byte < `0x90` |
| 3                | 2nd byte > `0x8F` |

- **UTF-16 Surrogate Pairs Boundaries:** The 1st byte equals `0xED` and the 2nd byte is between `0x80` and `0xBF`.
- **Truncation Safeguards:** Detects unexpected EOF mid-sequence.

## Limitations

- The program uses a simple filename string instead of an actual path. Which mean that the test files have to be inside the working directory.
- The program only read files and can't accepts folders or any raw bytes parsed into it.

## Roadmap / TODO

- [x] Implement robust **Overlong Encoding** traps (e.g., detecting `0xE0` followed by bytes lower than `0xA0`).
- [x] Add explicit UTF-16 Surrogate Pair filtering (blocking `0xED` sub-ranges)
- [x] Implement **set of target flags** to explicitly process directories, files, or text codes.
- [x] Refactor and secure native **stdin piping**.
- [x] Implement a granular `--debug` to output trace-level evaluation logs.
- [x] Add a `--show-bytes` flag to dump reconstructed strings to the console.
- [ ] Inhouse UTF-8 encoder when we want to display each reconstructed character

## License

This project is licensed under the **MIT License**.

See the [LICENSE](./LICENSE) file for the full license text.
