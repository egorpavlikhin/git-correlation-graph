// This file initializes A-Frame before it's needed by other dependencies
import 'aframe';

// Export a dummy function to ensure this file is not tree-shaken
export const initAFrame = () => {
  // A-Frame is initialized when imported
  console.log('A-Frame initialized');
};
