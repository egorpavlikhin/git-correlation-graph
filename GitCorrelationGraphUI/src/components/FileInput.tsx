import React, { useRef } from 'react';

interface FileInputProps {
  onFileLoaded: (file: File) => void;
}

const FileInput: React.FC<FileInputProps> = ({ onFileLoaded }) => {
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (files && files.length > 0) {
      onFileLoaded(files[0]);
    }
  };

  const handleClick = () => {
    fileInputRef.current?.click();
  };

  return (
    <div className="file-input-container">
      <h2>Load Correlation Graph</h2>
      <p>Select a JSON file containing the git correlation graph data:</p>
      <input
        type="file"
        ref={fileInputRef}
        onChange={handleFileChange}
        accept=".json"
        style={{ display: 'none' }}
      />
      <button onClick={handleClick} className="file-input-button">
        Select JSON File
      </button>
    </div>
  );
};

export default FileInput;
