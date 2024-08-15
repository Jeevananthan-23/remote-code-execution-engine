import axios from "axios";
import { LANGUAGE_VERSIONS } from "./constants";

const API = axios.create({
  baseURL: "http://localhost:5000",
});

export const executeCode = async (language, sourceCode) => {
  // Send the code submission request
  const response = await API.post("/submit", {
    src: LANGUAGE_VERSIONS[language],
    input: sourceCode,
    lang: language,
    timeout: 100000
  });

  const id = response.data;

  // Polling function to check the status
  const checkStatus = async () => {
    while (true) {
      // Get the current status
      const status = await API.get(`results/${id}`);

      // Check if the status is "Queued" or "Processing"
      if (status.data === "Queued" || status.data === "Processing") {
        // Wait for a while before making the next request (e.g., 3 second)
        await new Promise(resolve => setTimeout(resolve, 3000));
      } else {
        // If the status is not "Queued" or "Processing", return the result
        return status.data;
      }
    }
  };

  // Call the polling function and return its result
  return await checkStatus();
};

