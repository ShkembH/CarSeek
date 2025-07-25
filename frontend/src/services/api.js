const API_BASE_URL = 'http://localhost:5193/api';

class ApiService {
  async approveDealership(id) {
    const response = await fetch(`${this.baseURL}/Admin/dealerships/${id}/approve`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`,
      },
    });
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return { success: true };
  }

  async rejectDealership(id) {
    const response = await fetch(`${this.baseURL}/Admin/dealerships/${id}/reject`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`,
      },
    });
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return { success: true };
  }
  constructor() {
    this.baseURL = API_BASE_URL;
  }

  async request(endpoint, config = {}) {
    const url = `${this.baseURL}${endpoint}`;
    const token = localStorage.getItem('token');

    if (!config.headers) {
      config.headers = {};
    }

    config.headers['Content-Type'] = 'application/json';

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    try {
      const response = await fetch(url, config);

      if (!response.ok) {
        // Try to extract a specific error message from the response body
        let errorMessage = `HTTP error! status: ${response.status}`;
        try {
          const errorText = await response.text();
          if (errorText) {
            // Try to parse as JSON
            try {
              const errorJson = JSON.parse(errorText);
              if (errorJson) {
                // Handle errors as array or object
                if (Array.isArray(errorJson.errors)) {
                  errorMessage = errorJson.errors.join(' ');
                } else if (typeof errorJson.errors === 'object' && errorJson.errors !== null) {
                  // errors as object: { field: [msg1, msg2], ... }
                  errorMessage = Object.values(errorJson.errors).flat().join(' ');
                } else if (errorJson.message) {
                  errorMessage = errorJson.message;
                } else if (typeof errorJson === 'string') {
                  errorMessage = errorJson;
                }
              }
            } catch {
              // Not JSON, use as plain text
              errorMessage = errorText;
            }
          }
        } catch {}
        throw new Error(errorMessage);
      }

      // Check if response is empty (204 No Content) or has no content
      if (response.status === 204 || response.headers.get('content-length') === '0') {
        return { success: true };
      }

      // Check if response has content before trying to parse JSON
      const text = await response.text();
      if (!text) {
        return { success: true };
      }

      return JSON.parse(text);
    } catch (error) {
      console.error('API request failed:', error);
      throw error;
    }
  }

  // Auth endpoints
  async login(email, password) {
    console.log('🔐 [API] Calling login with email:', email);
    const response = await this.request('/Auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
    console.log('🔐 [API] Login response:', response);
    return response;
  }

  async register(userData) {
    console.log('🔐 [API] Calling register with data:', userData);
    
    // Create FormData for file upload
    const formData = new FormData();
    
    // Add all text fields
    formData.append('email', userData.email);
    formData.append('password', userData.password);
    formData.append('firstName', userData.firstName);
    formData.append('lastName', userData.lastName);
    formData.append('phoneNumber', userData.phoneNumber || '');
    formData.append('country', userData.country || '');
    formData.append('city', userData.city || '');
    formData.append('role', userData.role);
    
    // Add dealership-specific fields
    if (userData.role === 1 || userData.role === 'Dealership') {
      formData.append('companyName', userData.companyName || '');
      formData.append('companyUniqueNumber', userData.companyUniqueNumber || '');
      formData.append('location', userData.location || '');
      
      // Add business certificate file if provided
      if (userData.businessCertificate) {
        formData.append('businessCertificate', userData.businessCertificate);
      }
    }
    
    // Make the request with FormData
    const token = localStorage.getItem('token');
    const url = `${this.baseURL}/Auth/register`;
    
    try {
      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Authorization': token ? `Bearer ${token}` : '',
        },
        body: formData,
      });

      if (!response.ok) {
        let errorMessage = `HTTP error! status: ${response.status}`;
        try {
          const errorText = await response.text();
          if (errorText) {
            try {
              const errorJson = JSON.parse(errorText);
              if (errorJson) {
                if (Array.isArray(errorJson.errors)) {
                  errorMessage = errorJson.errors.join(' ');
                } else if (typeof errorJson.errors === 'object' && errorJson.errors !== null) {
                  errorMessage = Object.values(errorJson.errors).flat().join(' ');
                } else if (errorJson.message) {
                  errorMessage = errorJson.message;
                } else if (typeof errorJson === 'string') {
                  errorMessage = errorJson;
                }
              }
            } catch {
              errorMessage = errorText;
            }
          }
        } catch {}
        throw new Error(errorMessage);
      }

      const text = await response.text();
      if (!text) {
        return { success: true };
      }

      const result = JSON.parse(text);
      console.log('🔐 [API] Register response:', result);
      return result;
    } catch (error) {
      console.error('API request failed:', error);
      throw error;
    }
  }

  // Car Listings endpoints
  async getCarListings() {
    return this.request('/CarListings'); // Return the full response object
  }

  async getCarListing(id) {
    return this.request(`/CarListings/${id}`);
  }

  async createCarListing(carData) {
    console.log('🚗 [API] Creating car listing with data:', carData);
    const response = await this.request('/CarListings', {
      method: 'POST',
      body: JSON.stringify(carData),
    });
    console.log('🚗 [API] Create car listing response:', response);
    return response;
  }

  async updateCarListing(id, carData) {
    return this.request(`/CarListings/${id}`, {
      method: 'PUT',
      body: JSON.stringify(carData),
    });
  }

  async deleteCarListing(id) {
    return this.request(`/CarListings/${id}`, {
      method: 'DELETE',
    });
  }

  // Dealerships endpoints
  async getDealerships() {
    return this.request('/Dealerships');
  }

  async getDealership(id) {
    return this.request(`/Dealerships/${id}`);
  }

  async createDealership(dealershipData) {
    return this.request('/Dealerships', {
      method: 'POST',
      body: JSON.stringify(dealershipData),
    });
  }

  // Admin endpoints
  async getAdminStats() {
    console.log('🔍 [API] Calling getAdminStats...');
    const response = await this.request('/Admin/stats');
    console.log('📊 [API] getAdminStats response:', response);
    return response;
  }

  async getUsers() {
    console.log('🔍 [API] Calling getUsers...');
    const response = await this.request('/Admin/users');
    console.log('👥 [API] getUsers response:', response);
    return response;
  }

  async getAdminListings() {
    console.log('🔍 [API] Calling getAdminListings...');
    const response = await this.request('/Admin/listings');
    console.log('🚗 [API] getAdminListings response:', response);
    return response;
  }

  async getAdminDealerships() {
    console.log('🔍 [API] Calling getAdminDealerships...');
    const response = await this.request('/Admin/dealerships');
    console.log('🏢 [API] getAdminDealerships response:', response);
    return response;
  }

  async getPendingApprovals() {
    console.log('🔍 [API] Calling getPendingApprovals...');
    const response = await this.request('/Admin/pending-approvals');
    console.log('⏳ [API] getPendingApprovals response:', response);
    return response;
  }

  async getRecentActivity() {
    console.log('🔍 [API] Calling getRecentActivity...');
    const response = await this.request('/Admin/recent-activity');
    console.log('📝 [API] getRecentActivity response:', response);
    return response;
  }

  async approveListing(id) {
    const response = await fetch(`${this.baseURL}/Admin/listings/${id}/approve`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`,
      },
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    // Don't try to parse JSON for empty response
    return { success: true };
  }

  async rejectListing(id) {
    const response = await fetch(`${this.baseURL}/Admin/listings/${id}/reject`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`,
      },
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    // Don't try to parse JSON for empty response
    return { success: true };
  }

  // Add this new method
  async uploadCarImages(carId, images) {
    console.log('📸 [API] Uploading images for car ID:', carId);
    console.log('📸 [API] Number of images to upload:', images.length);

    // Convert File objects to base64
    const imagesData = await Promise.all(
      images.map(async (img, index) => {
        console.log(`📸 [API] Processing image ${index + 1}:`, {
          fileName: img.file?.name,
          fileSize: img.file?.size,
          fileType: img.file?.type
        });

        return {
          base64Image: await this.fileToBase64(img.file),
          altText: img.altText || '',
          displayOrder: index,
          isPrimary: index === 0 // First image is primary by default
        };
      })
    );

    console.log('📸 [API] Converted images to base64, sending to backend...');

    const response = await this.request(`/CarListings/${carId}/images`, {
      method: 'POST',
      body: JSON.stringify({ Images: imagesData }),
    });

    console.log('📸 [API] Upload response:', response);
    return response;
  }

  // Helper method to convert File to base64
  async fileToBase64(file) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve(reader.result);
      reader.onerror = error => reject(error);
    });
  }
  // Add this method after the other CarListings methods
  async getMyListings(pageNumber = 1, pageSize = 10) {
    return this.request(`/CarListings/my-listings?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  // Saved Listings endpoints
  async saveListing(listingId) {
    return this.request(`/SavedListings/${listingId}`, { method: 'POST' });
  }
  async unsaveListing(listingId) {
    return this.request(`/SavedListings/${listingId}`, { method: 'DELETE' });
  }
  async getSavedListings() {
    return this.request('/SavedListings');
  }

  // Profile endpoints
  async getProfile() {
    return this.request('/profile');
  }

  async updateProfile(formData) {
    const token = localStorage.getItem('token');
    const response = await fetch(`${this.baseURL}/profile`, {
      method: 'PUT',
      headers: {
        Authorization: token ? `Bearer ${token}` : undefined,
        // Do NOT set Content-Type for FormData; browser will set it
      },
      body: formData,
    });
    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(errorText || `HTTP error! status: ${response.status}`);
    }
    return true;
  }

  async deleteUser(userId) {
    const token = localStorage.getItem('token');
    const response = await fetch(`${this.baseURL}/Admin/users/${userId}`, {
      method: 'DELETE',
      headers: {
        'Authorization': token ? `Bearer ${token}` : undefined,
      },
    });
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return true;
  }

  async deleteAllListingsByUser(userId) {
    return this.request(`/CarListings/user/${userId}`, {
      method: 'DELETE',
    });
  }

  async updateCarListing(id, data) {
    if (data instanceof FormData) {
      return this.request(`/CarListings/${id}`, {
        method: 'PUT',
        headers: {
          // Remove Content-Type header to let the browser set it with the boundary
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: data
      });
    } else {
      return this.request(`/CarListings/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data)
      });
    }
  }

  async getCarListingById(id) {
    try {
      console.log('Fetching car listing with ID:', id); // Debug log
      const response = await this.request(`/CarListings/${id}`, {
        method: 'GET'
      });
      console.log('Received response:', response); // Debug log
      return response;
    } catch (error) {
      console.error('Error fetching car listing:', error);
      throw error;
    }
  }
}

export const apiService = new ApiService();
export const getProfile = (...args) => apiService.getProfile(...args);
export const updateProfile = (...args) => apiService.updateProfile(...args);

apiService.getUserById = async (userId) => {
  return await apiService.request(`/Admin/users/${userId}`);
};

apiService.getCarListingsByUser = async (userId) => {
  return await apiService.request(`/CarListings/user/${userId}`);
};
