import React, { useState } from 'react';
import axios from 'axios';

const FileUpload = () => {
    const [file, setFile] = useState(null);

    const handleFileChange = (e) => {
        setFile(e.target.files[0]);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        const formData = new FormData();
        formData.append('file', file);

        try {
            const response = await axios.post('/api/files', formData, {
                headers: { 'Content-Type': 'multipart/form-data' },
            });

            alert('File uploaded successfully!');
        } catch (error) {
            console.error(error);
            alert('Failed to upload file.');
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <div className="mb-3">
                <label htmlFor="file" className="form-label">Choose file:</label>
                <input type="file" className="form-control" id="file" onChange={handleFileChange} />
            </div>
            <button type="submit" className="btn btn-primary">Upload</button>
        </form>
    );
};

export default FileUpload;
