import React, { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiService } from '../services/api';
import '../styles/pages/AddListing.css';
import carData from '../carData';

const AddListing = () => {
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  const [currentStep, setCurrentStep] = useState(1);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errors, setErrors] = useState({});
  const fileInputRef = useRef(null);

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    year: '',
    make: '', // Brand
    series: '', // Series/Family (e.g., C)
    carClass: '', // Class/Body Type (e.g., C-Class, CLA, etc.)
    price: '',
    mileage: '',
    condition: 'used',
    fuelType: 'gasoline',
    transmission: 'manual',
    color: '',
    features: [],
    images: []
  });

  const totalSteps = 5; // Updated to 5 steps

  // Remove carMakesAndModels


  const conditions = [
    { value: 'new', label: 'New' },
    { value: 'used', label: 'Used' }
  ];

  const fuelTypes = [
    { value: 'gasoline', label: 'Gasoline' },
    { value: 'diesel', label: 'Diesel' },
    { value: 'electric', label: 'Electric' },
    { value: 'hybrid', label: 'Hybrid' }
  ];

  const transmissionTypes = [
    { value: 'manual', label: 'Manual' },
    { value: 'automatic', label: 'Automatic' },
    { value: 'cvt', label: 'CVT' }
  ];

  const carColors = [
    { value: 'black', label: 'Black' },
    { value: 'white', label: 'White' },
    { value: 'silver', label: 'Silver' },
    { value: 'gray', label: 'Gray' },
    { value: 'red', label: 'Red' },
    { value: 'blue', label: 'Blue' },
    { value: 'green', label: 'Green' },
    { value: 'yellow', label: 'Yellow' },
    { value: 'orange', label: 'Orange' },
    { value: 'brown', label: 'Brown' },
    { value: 'purple', label: 'Purple' },
    { value: 'gold', label: 'Gold' },
    { value: 'beige', label: 'Beige' },
    { value: 'other', label: 'Other' }
  ];

  const availableFeatures = [
    'Air Conditioning', 'GPS Navigation', 'Bluetooth', 'Backup Camera',
    'Heated Seats', 'Sunroof', 'Leather Seats', 'Cruise Control',
    'Parking Sensors', 'Keyless Entry', 'Premium Sound System', 'USB Ports'
  ];

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    if (name === 'make') {
      setFormData(prev => ({
        ...prev,
        make: value,
        series: '',
        carClass: ''
      }));
    } else if (name === 'series') {
      setFormData(prev => ({
        ...prev,
        series: value,
        carClass: ''
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: value
      }));
    }
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
    if ((name === 'make' || name === 'series') && errors.carClass) {
      setErrors(prev => ({ ...prev, carClass: '' }));
    }
  };

  const getAvailableSeries = () => {
    return formData.make ? Object.keys(carData[formData.make] || {}) : [];
  };
  const getAvailableClasses = () => {
    return formData.make && formData.series ? carData[formData.make][formData.series] || [] : [];
  };

  const handleFeatureToggle = (feature) => {
    setFormData(prev => ({
      ...prev,
      features: prev.features.includes(feature)
        ? prev.features.filter(f => f !== feature)
        : [...prev.features, feature]
    }));
  };

  const validateStep = (step) => {
    const newErrors = {};

    switch (step) {
      case 1:
        if (!formData.make.trim()) newErrors.make = 'Brand is required';
        if (!formData.series.trim()) newErrors.series = 'Series is required';
        // carClass is optional
        if (!formData.year || formData.year < 1900 || formData.year > new Date().getFullYear() + 1) {
          newErrors.year = 'Please enter a valid year';
        }
        break;
      case 2:
        if (!formData.condition) newErrors.condition = 'Please select condition';
        if (!formData.mileage || formData.mileage < 0) newErrors.mileage = 'Please enter valid mileage';
        if (!formData.fuelType) newErrors.fuelType = 'Please select fuel type';
        if (!formData.transmission) newErrors.transmission = 'Please select transmission';
        break;
      case 3:
        if (!formData.price || formData.price <= 0) newErrors.price = 'Please enter a valid price';
        if (!formData.title.trim()) newErrors.title = 'Title is required';
        if (!formData.description.trim()) newErrors.description = 'Description is required';
        break;
      case 4:
        if (formData.images.length === 0) {
          newErrors.images = 'Please add at least one image of your vehicle';
        }
        break;
      default:
        break;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Image handling functions
  const handleImageUpload = (e) => {
    const files = Array.from(e.target.files);
    const maxFiles = 10;
    const maxSize = 5 * 1024 * 1024; // 5MB per file

    if (formData.images.length + files.length > maxFiles) {
      setErrors(prev => ({ ...prev, images: `Maximum ${maxFiles} images allowed` }));
      return;
    }

    const validFiles = [];
    const invalidFiles = [];

    files.forEach(file => {
      if (file.size > maxSize) {
        invalidFiles.push(file.name);
      } else if (!file.type.startsWith('image/')) {
        invalidFiles.push(file.name);
      } else {
        validFiles.push(file);
      }
    });

    if (invalidFiles.length > 0) {
      setErrors(prev => ({
        ...prev,
        images: `Invalid files: ${invalidFiles.join(', ')}. Please use images under 5MB.`
      }));
      return;
    }

    // Create preview URLs for valid files
    const newImages = validFiles.map(file => ({
      file,
      preview: URL.createObjectURL(file),
      id: Date.now() + Math.random()
    }));

    setFormData(prev => ({
      ...prev,
      images: [...prev.images, ...newImages]
    }));

    // Clear any previous errors
    if (errors.images) {
      setErrors(prev => ({ ...prev, images: '' }));
    }
  };

  const removeImage = (imageId) => {
    setFormData(prev => {
      const updatedImages = prev.images.filter(img => img.id !== imageId);
      // Revoke object URL to prevent memory leaks
      const imageToRemove = prev.images.find(img => img.id === imageId);
      if (imageToRemove && imageToRemove.preview) {
        URL.revokeObjectURL(imageToRemove.preview);
      }
      return {
        ...prev,
        images: updatedImages
      };
    });
  };

  const reorderImages = (dragIndex, hoverIndex) => {
    const draggedImage = formData.images[dragIndex];
    const newImages = [...formData.images];
    newImages.splice(dragIndex, 1);
    newImages.splice(hoverIndex, 0, draggedImage);

    setFormData(prev => ({
      ...prev,
      images: newImages
    }));
  };

  // Add a function to set main image
  const setMainImage = (imageId) => {
    setFormData(prev => {
      const idx = prev.images.findIndex(img => img.id === imageId);
      if (idx <= 0) return prev; // Already main or not found
      const newImages = [...prev.images];
      const [mainImg] = newImages.splice(idx, 1);
      newImages.unshift(mainImg);
      return { ...prev, images: newImages };
    });
  };

  const nextStep = () => {
    if (validateStep(currentStep)) {
      setCurrentStep(prev => Math.min(prev + 1, totalSteps));
    }
  };

  const prevStep = () => {
    setCurrentStep(prev => Math.max(prev - 1, 1));
  };

  const handleSubmit = async () => {
    if (!validateStep(currentStep)) return;

    setIsSubmitting(true);
    try {
      // Always send a model value: carClass if selected, otherwise series
      const model = formData.carClass || formData.series;
      const listingData = {
        title: formData.title,
        description: formData.description,
        year: parseInt(formData.year),
        make: formData.make,
        model: model,
        price: parseFloat(formData.price),
        mileage: parseInt(formData.mileage),
        fuelType: formData.fuelType,
        transmission: formData.transmission,
        color: formData.color,
        features: JSON.stringify(formData.features) // Convert features array to JSON string
      };

      console.log('[DEBUG] Features being sent:', formData.features);
      console.log('[DEBUG] Features JSON string:', JSON.stringify(formData.features));
      console.log('[DEBUG] Full listing data being sent:', listingData);

      // Create the car listing and get the response which should contain the ID
      const response = await apiService.createCarListing(listingData);

      // If there are images to upload and we have a car ID
      if (formData.images.length > 0 && response.id) {
        try {
          // Upload the images for this car listing
          await apiService.uploadCarImages(response.id, formData.images);
        } catch (imageError) {
          console.error('Error uploading images:', imageError);
          // Continue with navigation even if image upload fails
        }
      }

      // Show appropriate message based on user role
      const isAdmin = user?.role === 'Admin';
      const isDealership = user?.role === 'Dealership';

      let message = 'Listing created successfully!';
      if (isAdmin || isDealership) {
        message = 'Listing created and automatically approved!';
      } else {
        message = 'Listing created successfully! It will be reviewed by an admin before being published.';
      }

      navigate('/dashboard', { state: { message } });
    } catch (error) {
      console.error('Error creating listing:', error);
      setErrors({ submit: 'Failed to create listing. Please try again.' });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleStepClick = (step) => {
    // Only allow clicking on current step, previous steps, or next step if current is valid
    if (step <= currentStep) {
      // Can always go back to previous steps
      setCurrentStep(step);
    } else if (step === currentStep + 1) {
      // Can go to next step only if current step is valid
      if (validateStep(currentStep)) {
        setCurrentStep(step);
      }
    }
    // Steps beyond next step are disabled and not clickable
  };

  // Add modal state for image preview
  const [previewImage, setPreviewImage] = useState(null);

  if (!isAuthenticated) {
    return (
      <div className="add-listing-container">
        <div className="add-listing-auth-required">
          <h2>Authentication Required</h2>
          <p>Please log in to create a listing.</p>
          <button
            className="btn btn-primary"
            onClick={() => navigate('/login')}
          >
            Go to Login
          </button>
        </div>
      </div>
    );
  }

  const renderProgressBar = () => (
    <div className="add-listing-progress">
      <div className="add-listing-progress-bar">
        {[1, 2, 3, 4, 5].map(step => (
          <div
            key={step}
            className={`add-listing-progress-step ${
              step <= currentStep ? 'active' : ''
            } ${step < currentStep ? 'completed' : ''} ${
              step <= currentStep || step === currentStep + 1 ? 'clickable' : 'disabled'
            }`}
            onClick={() => handleStepClick(step)}
          >
            <div className="add-listing-progress-circle">
              {step < currentStep ? '✓' : <span className="progress-dot" />}
            </div>
            <span className="add-listing-progress-label">
              {step === 1 && 'Vehicle Info'}
              {step === 2 && 'Details'}
              {step === 3 && 'Listing Info'}
              {step === 4 && 'Photos'}
              {step === 5 && 'Review'}
            </span>
          </div>
        ))}
      </div>
    </div>
  );

  const renderStep1 = () => (
    <div className="add-listing-step">
      <h3>Vehicle Information</h3>
      <div className="row">
        <div className="col-md-4">
          <div className="mb-3">
            <label className="form-label">Brand *</label>
            <select
              className={`form-select ${errors.make ? 'is-invalid' : ''}`}
              name="make"
              value={formData.make}
              onChange={handleInputChange}
            >
              <option value="">Select a brand</option>
              {Object.keys(carData).sort().map(make => (
                <option key={make} value={make}>{make}</option>
              ))}
            </select>
            {errors.make && <div className="invalid-feedback">{errors.make}</div>}
          </div>
        </div>
        <div className="col-md-4">
          <div className="mb-3">
            <label className="form-label">Series *</label>
            <select
              className={`form-select ${errors.series ? 'is-invalid' : ''}`}
              name="series"
              value={formData.series}
              onChange={handleInputChange}
              disabled={!formData.make}
            >
              <option value="">{formData.make ? 'Select a series' : 'First select a brand'}</option>
              {getAvailableSeries().map(series => (
                <option key={series} value={series}>{series}</option>
              ))}
            </select>
            {errors.series && <div className="invalid-feedback">{errors.series}</div>}
            {!formData.make && (
              <div className="form-text text-muted">Please select a brand first to see available series</div>
            )}
          </div>
        </div>
        <div className="col-md-4">
          <div className="mb-3">
            <label className="form-label">Class/Body Type (optional)</label>
            <select
              className="form-select"
              name="carClass"
              value={formData.carClass}
              onChange={handleInputChange}
              disabled={!formData.series || getAvailableClasses().length === 0}
            >
              <option value="">{formData.series ? 'Select a class/body type (optional)' : 'First select a series'}</option>
              {getAvailableClasses().map(carClass => (
                <option key={carClass} value={carClass}>{carClass}</option>
              ))}
            </select>
          </div>
        </div>
      </div>
      <div className="row">
        <div className="col-md-6">
          <div className="mb-3">
            <label className="form-label">Year *</label>
            <input
              type="number"
              className={`form-control ${errors.year ? 'is-invalid' : ''}`}
              name="year"
              value={formData.year}
              onChange={handleInputChange}
              min="1900"
              max={new Date().getFullYear() + 1}
            />
            {errors.year && <div className="invalid-feedback">{errors.year}</div>}
          </div>
        </div>
        <div className="col-md-6">
          <div className="mb-3">
            <label className="form-label">Color</label>
            <select
              className="form-select"
              name="color"
              value={formData.color}
              onChange={handleInputChange}
            >
              <option value="">Select a color</option>
              {carColors.map(color => (
                <option key={color.value} value={color.value}>{color.label}</option>
              ))}
            </select>
          </div>
        </div>
      </div>
    </div>
  );

  const renderStep2 = () => (
    <div className="add-listing-step">
      <h3>Vehicle Details</h3>

      <div className="row">
        <div className="col-md-6">
          <div className="mb-3">
            <label className="form-label">Condition *</label>
            <select
              className={`form-select ${errors.condition ? 'is-invalid' : ''}`}
              name="condition"
              value={formData.condition}
              onChange={handleInputChange}
            >
              {conditions.map(condition => (
                <option key={condition.value} value={condition.value}>
                  {condition.label}
                </option>
              ))}
            </select>
            {errors.condition && <div className="invalid-feedback">{errors.condition}</div>}
          </div>
        </div>
        <div className="col-md-6">
          <div className="mb-3">
            <label className="form-label">Mileage (km) *</label>
            <input
              type="number"
              className={`form-control ${errors.mileage ? 'is-invalid' : ''}`}
              name="mileage"
              value={formData.mileage}
              onChange={handleInputChange}
              min="0"
              placeholder="e.g., 50000"
            />
            {errors.mileage && <div className="invalid-feedback">{errors.mileage}</div>}
          </div>
        </div>
      </div>

      <div className="row">
        <div className="col-md-6">
          <div className="mb-3">
            <label className="form-label">Fuel Type *</label>
            <select
              className={`form-select ${errors.fuelType ? 'is-invalid' : ''}`}
              name="fuelType"
              value={formData.fuelType}
              onChange={handleInputChange}
            >
              {fuelTypes.map(fuel => (
                <option key={fuel.value} value={fuel.value}>
                  {fuel.label}
                </option>
              ))}
            </select>
            {errors.fuelType && <div className="invalid-feedback">{errors.fuelType}</div>}
          </div>
        </div>
        <div className="col-md-6">
          <div className="mb-3">
            <label className="form-label">Transmission *</label>
            <select
              className={`form-select ${errors.transmission ? 'is-invalid' : ''}`}
              name="transmission"
              value={formData.transmission}
              onChange={handleInputChange}
            >
              {transmissionTypes.map(trans => (
                <option key={trans.value} value={trans.value}>
                  {trans.label}
                </option>
              ))}
            </select>
            {errors.transmission && <div className="invalid-feedback">{errors.transmission}</div>}
          </div>
        </div>
      </div>

      <div className="mb-3">
        <label className="form-label">Features</label>
        <div className="add-listing-features-grid">
          {availableFeatures.map(feature => (
            <div
              key={feature}
              className={`add-listing-feature-item ${
                formData.features.includes(feature) ? 'selected' : ''
              }`}
              onClick={() => handleFeatureToggle(feature)}
            >
              <span>{feature}</span>
              {formData.features.includes(feature) && <span className="checkmark">✓</span>}
            </div>
          ))}
        </div>
      </div>
    </div>
  );

  const renderStep3 = () => (
    <div className="add-listing-step">
      <h3>Listing Information</h3>

      <div className="mb-3">
        <label className="form-label">Price (€) *</label>
        <input
          type="number"
          className={`form-control ${errors.price ? 'is-invalid' : ''}`}
          name="price"
          value={formData.price}
          onChange={handleInputChange}
          min="0"
          step="0.01"
          placeholder="e.g., 25000"
        />
        {errors.price && <div className="invalid-feedback">{errors.price}</div>}
      </div>

      <div className="mb-3">
        <label className="form-label">Title *</label>
        <input
          type="text"
          className={`form-control ${errors.title ? 'is-invalid' : ''}`}
          name="title"
          value={formData.title}
          onChange={handleInputChange}
          placeholder="e.g., 2020 Toyota Camry - Excellent Condition"
          maxLength="100"
        />
        {errors.title && <div className="invalid-feedback">{errors.title}</div>}
        <div className="form-text">{formData.title.length}/100 characters</div>
      </div>

      <div className="mb-3">
        <label className="form-label">Description *</label>
        <textarea
          className={`form-control ${errors.description ? 'is-invalid' : ''}`}
          name="description"
          value={formData.description}
          onChange={handleInputChange}
          rows="5"
          placeholder="Describe your vehicle in detail. Include any special features, maintenance history, or other relevant information..."
          maxLength="1000"
        />
        {errors.description && <div className="invalid-feedback">{errors.description}</div>}
        <div className="form-text">{formData.description.length}/1000 characters</div>
      </div>
    </div>
  );

  const renderStep4 = () => (
    <div className="add-listing-step">
      <h3>Add Photos</h3>
      <p className="text-muted mb-4">
        Add high-quality photos of your vehicle. The first photo will be used as the main image.
      </p>

      <div className="add-listing-image-upload">
        <input
          type="file"
          ref={fileInputRef}
          onChange={handleImageUpload}
          multiple
          accept="image/*"
          style={{ display: 'none' }}
        />

        <div className="add-listing-upload-area" onClick={() => fileInputRef.current?.click()}>
          <div className="add-listing-upload-content">
            <div className="add-listing-upload-icon">📷</div>
            <h5>Click to upload photos</h5>
            <p>or drag and drop images here</p>
            <small className="text-muted">
              Maximum 10 images, 5MB each. Supported formats: JPG, PNG, GIF
            </small>
          </div>
        </div>

        {errors.images && <div className="add-listing-error mt-2">{errors.images}</div>}
      </div>

      {formData.images.length > 0 && (
        <div className="add-listing-image-preview">
          <h5>Uploaded Images ({formData.images.length}/10)</h5>
          <div className="add-listing-image-grid">
            {formData.images.map((image, index) => (
              <div key={image.id} className={`add-listing-image-item${index === 0 ? ' main' : ''}`}>
                {/* Main image radio selector */}
                <div
                  className={`add-listing-main-radio${index === 0 ? ' selected' : ''}`}
                  onClick={() => index !== 0 && setMainImage(image.id)}
                  title={index === 0 ? 'Main Photo' : 'Set as Main Photo'}
                >
                  <div className="add-listing-main-radio-inner" />
                </div>
                <img src={image.preview} alt={`Preview ${index + 1}`} />
                <div className="add-listing-image-overlay">
                  {/* No main badge */}
                  <button
                    type="button"
                    className="add-listing-remove-image"
                    onClick={() => removeImage(image.id)}
                  >
                    ×
                  </button>
                </div>
                <div className="add-listing-image-actions">
                  {index > 0 && (
                    <button
                      type="button"
                      className="btn btn-sm btn-outline-primary"
                      onClick={() => reorderImages(index, index - 1)}
                    >
                      ← Move Left
                    </button>
                  )}
                  {index < formData.images.length - 1 && (
                    <button
                      type="button"
                      className="btn btn-sm btn-outline-primary"
                      onClick={() => reorderImages(index, index + 1)}
                    >
                      Move Right →
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
          <p className="text-muted mt-2">
            <small>Tip: Click the radio button to select the main photo. The first image will be used as the main photo in your listing.</small>
          </p>
        </div>
      )}
    </div>
  );

  const renderStep5 = () => (
    <div className="add-listing-step">
      <h3>Review Your Listing</h3>

      <div className="add-listing-review">
        <div className="add-listing-review-section">
          <h5>Vehicle Information</h5>
          <div className="add-listing-review-grid">
            <div><strong>Brand:</strong> {formData.make}</div>
            <div><strong>Series:</strong> {formData.series}</div>
            <div><strong>Class/Body Type:</strong> {formData.carClass || 'Not specified'}</div>
            <div><strong>Year:</strong> {formData.year}</div>
            <div><strong>Color:</strong> {formData.color || 'Not specified'}</div>
            <div><strong>Condition:</strong> {formData.condition}</div>
            <div><strong>Mileage:</strong> {formData.mileage} km</div>
            <div><strong>Fuel Type:</strong> {formData.fuelType}</div>
            <div><strong>Transmission:</strong> {formData.transmission}</div>
          </div>
        </div>

        <div className="add-listing-review-section">
          <h5>Listing Details</h5>
          <div><strong>Price:</strong> €{formData.price}</div>
          <div><strong>Title:</strong> {formData.title}</div>
          <div><strong>Description:</strong></div>
          <p className="add-listing-description-preview">{formData.description}</p>
        </div>

        {formData.features.length > 0 && (
          <div className="add-listing-review-section">
            <h5>Features</h5>
            <div className="add-listing-features-list">
              {formData.features.map(feature => (
                <span key={feature} className="add-listing-feature-tag">{feature}</span>
              ))}
            </div>
          </div>
        )}

        {formData.images.length > 0 && (
          <div className="add-listing-review-section">
            <h5>Photos ({formData.images.length})</h5>
            <div className="add-listing-review-images">
              {formData.images.slice(0, 4).map((image, index) => (
                <img
                  key={image.id}
                  src={image.preview}
                  alt={`Preview ${index + 1}`}
                  className="add-listing-review-image"
                  style={{ cursor: 'pointer' }}
                  onClick={() => setPreviewImage(image.preview)}
                />
              ))}
              {formData.images.length > 4 && (
                <div className="add-listing-more-images">
                  +{formData.images.length - 4} more
                </div>
              )}
            </div>
          </div>
        )}
        {/* Image preview modal */}
        {previewImage && (
          <div className="add-listing-image-modal" onClick={() => setPreviewImage(null)}>
            <div className="add-listing-image-modal-content" onClick={e => e.stopPropagation()}>
              <button className="add-listing-image-modal-close" onClick={() => setPreviewImage(null)}>&times;</button>
              <img src={previewImage} alt="Full Preview" />
            </div>
          </div>
        )}

        {errors.submit && (
          <div className="alert alert-danger">{errors.submit}</div>
        )}
      </div>
    </div>
  );

  const renderStepContent = () => {
    switch (currentStep) {
      case 1: return renderStep1();
      case 2: return renderStep2();
      case 3: return renderStep3();
      case 4: return renderStep4();
      case 5: return renderStep5();
      default: return renderStep1();
    }
  };

  return (
    <div className="add-listing-container">
      <div className="add-listing-header">
        <h1>Add New Listing</h1>
        <p>Create a detailed listing for your vehicle</p>
      </div>

      {renderProgressBar()}

      <div className="add-listing-card">
        <div className="add-listing-card-body">
          {renderStepContent()}
        </div>

        <div className="add-listing-card-footer">
          <div className="add-listing-navigation space-between">
            <div>
              {currentStep > 1 && (
                <button
                  type="button"
                  className="listings-btn"
                  onClick={prevStep}
                  disabled={isSubmitting}
                >
                  Previous
                </button>
              )}
            </div>
            <div>
              {currentStep < totalSteps ? (
                <button
                  type="button"
                  className="listings-btn"
                  onClick={nextStep}
                  disabled={isSubmitting}
                >
                  Next
                </button>
              ) : (
                <button
                  type="button"
                  className="listings-btn"
                  onClick={handleSubmit}
                  disabled={isSubmitting}
                >
                  {isSubmitting ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2" />
                      Creating...
                    </>
                  ) : (
                    'Create Listing'
                  )}
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AddListing;
