/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.razor",
    "../NexaCRM.WebServer/**/*.razor",
    "../NexaCRM.WebClient/**/*.razor",
    "../NexaCRM.WebServer/**/*.cshtml",
    "../NexaCRM.WebClient/wwwroot/index.html"
  ],
  theme: {
    extend: {},
  },
  plugins: [],
};
