/**
 * jsdom không có Fetch API (`Request`, `Response`, `Headers`, `fetch`) mà `ky` cần (Buổi 6 — lớp gọi API).
 * Môi trường này là jsdom + đúng các lớp Fetch của Node 22 lấy từ realm ngoài.
 *
 * `AbortController`/`AbortSignal` cũng phải lấy của Node: `Request` của Node từ chối `signal` của jsdom
 * ("member signal is not of type AbortSignal"). `ReadableStream` đi cùng vì body của Response là stream của Node;
 * `FormData`/`Blob`/`File` đi cùng vì `Request` của Node không nhận ra FormData của jsdom (gửi thành "[object FormData]").
 * Kiểm thử nào gán `global.fetch = jest.fn()` vẫn ghi đè được như trước.
 */
const { TestEnvironment } = require('jest-environment-jsdom');

const FETCH_GLOBALS = [
  'fetch',
  'Request',
  'Response',
  'Headers',
  'AbortController',
  'AbortSignal',
  'ReadableStream',
  'FormData',
  'Blob',
  'File',
  'TextEncoder',
  'TextDecoder',
];

class JsdomFetchEnvironment extends TestEnvironment {
  constructor(config, context) {
    super(config, context);
    FETCH_GLOBALS.forEach((name) => {
      this.global[name] = globalThis[name];
    });
  }
}

module.exports = JsdomFetchEnvironment;
