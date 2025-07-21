import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { apiService } from '../services/api';
import '../styles/pages/EditListing.css';

const EditListing = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [images, setImages] = useState([]); // For existing images from server
  const [newImages, setNewImages] = useState([]); // For newly uploaded images
  const [removedImages, setRemovedImages] = useState([]); // For tracking images to remove
  const [primaryImageId, setPrimaryImageId] = useState(null);

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    price: '',
    make: '',
    model: '',
    year: '',
    mileage: '',
    color: '',
    fuelType: '',
    transmission: ''
  });

  useEffect(() => {
    fetchListing();
  }, [id]);

  const fetchListing = async () => {
    try {
      const listing = await apiService.getCarListingById(id);
      setFormData({
        title: listing.title,
        description: listing.description,
        price: listing.price,
        make: listing.make,
        model: listing.model,
        year: listing.year,
        mileage: listing.mileage,
        color: listing.color,
        fuelType: listing.fuelType,
        transmission: listing.transmission
      });
      const existingImages = listing.images || [];
      setImages(existingImages);
      // Set the primary image if one exists
      const primaryImage = existingImages.find(img => img.isPrimary);
      if (primaryImage) {
        setPrimaryImageId(primaryImage.id);
      }
      setLoading(false);
    } catch (error) {
      setError('Failed to load listing');
      console.error('Error loading listing:', error);
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleImageUpload = (e) => {
    const files = Array.from(e.target.files);
    const validFiles = files.filter(file => file.type.startsWith('image/'));
    
    if (validFiles.length !== files.length) {
      setError('Some files were skipped because they are not images.');
    }

    setNewImages(prev => [...prev, ...validFiles]);
  };

  const handleRemoveNewImage = (indexToRemove) => {
    setNewImages(prev => prev.filter((_, index) => index !== indexToRemove));
  };

  const handleRemoveExistingImage = (image) => {
    if (primaryImageId === image.id) {
      setPrimaryImageId(null);
    }
    setImages(prev => prev.filter(img => img.id !== image.id));
    setRemovedImages(prev => [...prev, image.id]);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      // First, handle image changes
      const formDataWithFiles = new FormData();
      
      // Add all form fields
      Object.entries(formData).forEach(([key, value]) => {
        formDataWithFiles.append(key, value);
      });

      // Add new images
      newImages.forEach((file, index) => {
        formDataWithFiles.append(`newImages`, file);
        // If this is set to be the primary image, send its index
        if (primaryImageId === `new-${index}`) {
          formDataWithFiles.append('primaryImageIndex', index.toString());
        }
      });

      // Add list of images to remove
      removedImages.forEach(imageId => {
        formDataWithFiles.append('removedImages', imageId);
      });

      // Add remaining existing images and mark primary
      images.forEach(image => {
        formDataWithFiles.append('existingImages', image.id);
        if (image.id === primaryImageId) {
          formDataWithFiles.append('primaryImageId', image.id);
        }
      });

      const updatedListing = await apiService.updateCarListing(id, formDataWithFiles);
      navigate(`/listings/${updatedListing.id}`);
    } catch (error) {
      setError('Failed to update listing. Please try again.');
      console.error('Error updating listing:', error);
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="container mt-5">
        <div className="text-center">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="edit-listing-page">
      <div className="container">
        <div className="page-header">
          <h1>Edit Listing</h1>
          <button onClick={() => navigate('/my-listings')} className="back-button">
            Back to My Listings
          </button>
        </div>

        {error && <div className="alert alert-danger">{error}</div>}

        <form onSubmit={handleSubmit} className="edit-form">
          <div className="form-section">
            <h2>Basic Information</h2>
            <div className="form-group">
              <label>Title</label>
              <input
                type="text"
                name="title"
                value={formData.title}
                onChange={handleChange}
                required
              />
            </div>

            <div className="form-group">
              <label>Description</label>
              <textarea
                name="description"
                value={formData.description}
                onChange={handleChange}
                required
              />
            </div>
          </div>

          <div className="form-section">
            <h2>Car Details</h2>
            <div className="form-row">
              <div className="form-group">
                <label>Make</label>
                <input
                  type="text"
                  name="make"
                  value={formData.make}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="form-group">
                <label>Model</label>
                <input
                  type="text"
                  name="model"
                  value={formData.model}
                  onChange={handleChange}
                  required
                />
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Year</label>
                <input
                  type="number"
                  name="year"
                  value={formData.year}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="form-group">
                <label>Price</label>
                <input
                  type="number"
                  name="price"
                  value={formData.price}
                  onChange={handleChange}
                  required
                />
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Mileage</label>
                <input
                  type="number"
                  name="mileage"
                  value={formData.mileage}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="form-group">
                <label>Color</label>
                <input
                  type="text"
                  name="color"
                  value={formData.color}
                  onChange={handleChange}
                  required
                />
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Fuel Type</label>
                <select name="fuelType" value={formData.fuelType} onChange={handleChange} required>
                  <option value="">Select Fuel Type</option>
                  <option value="Petrol">Petrol</option>
                  <option value="Diesel">Diesel</option>
                  <option value="Electric">Electric</option>
                  <option value="Hybrid">Hybrid</option>
                </select>
              </div>

              <div className="form-group">
                <label>Transmission</label>
                <select name="transmission" value={formData.transmission} onChange={handleChange} required>
                  <option value="">Select Transmission</option>
                  <option value="Manual">Manual</option>
                  <option value="Automatic">Automatic</option>
                </select>
              </div>
            </div>
          </div>

          <div className="form-section">
            <h2>Images</h2>
            <div className="image-upload-section">
              <div className="existing-images">
                <h3>Current Images</h3>
                <div className="image-grid">
                  {images.map((image, index) => (
                    <div key={image.id} className="image-item">
                      <img src={image.imageUrl} alt={image.altText || `Car view ${index + 1}`} />
                      <div className="image-actions">
                        <button
                          type="button"
                          onClick={() => handleRemoveExistingImage(image)}
                          className="remove-image"
                        >
                          Remove
                        </button>
                        <button
                          type="button"
                          onClick={() => setPrimaryImageId(image.id)}
                          className={`set-primary-image ${primaryImageId === image.id ? 'is-primary' : ''}`}
                        >
                          {primaryImageId === image.id ? 'Primary Image' : 'Set as Primary'}
                        </button>
                      </div>
                      {primaryImageId === image.id && (
                        <div className="primary-badge">Primary</div>
                      )}
                    </div>
                  ))}
                </div>
              </div>

              <div className="new-images">
                <h3>Add New Images</h3>
                <input
                  type="file"
                  multiple
                  accept="image/*"
                  onChange={handleImageUpload}
                  className="file-input"
                />
                <div className="image-preview-grid">
                  {newImages.map((file, index) => (
                    <div key={index} className="image-preview-item">
                      <img src={URL.createObjectURL(file)} alt={`New upload ${index + 1}`} />
                      <button
                        type="button"
                        onClick={() => handleRemoveNewImage(index)}
                        className="remove-image"
                      >
                        Remove
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>

          <div className="form-actions">
            <button type="button" onClick={() => navigate('/my-listings')} className="cancel-button">
              Cancel
            </button>
            <button type="submit" className="save-button" disabled={loading}>
              {loading ? 'Saving Changes...' : 'Save Changes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default EditListing;
