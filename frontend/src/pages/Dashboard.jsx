import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { apiService } from '../services/api';
import '../styles/pages/AdminDashboard.css';

const Dashboard = () => {
  // --- Hooks & State ---
  const navigate = useNavigate();
  const { isAuthenticated, isAdmin } = useAuth();
  const [activeTab, setActiveTab] = useState('overview');
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState({
    totalUsers: 0,
    totalListings: 0,
    totalDealerships: 0,
    pendingApprovals: 0,
    monthlyRevenue: 0,
    activeUsers: 0
  });
  const [users, setUsers] = useState([]);
  const [listings, setListings] = useState([]);
  const [dealerships, setDealerships] = useState([]);
  const [pendingApprovals, setPendingApprovals] = useState([]);
  const [activityLogs, setActivityLogs] = useState([]);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [listingToDelete, setListingToDelete] = useState(null);
  // User filter state
  const [userSearch, setUserSearch] = useState('');
  const [userRole, setUserRole] = useState('');
  // Listing filter state
  const [listingSearch, setListingSearch] = useState('');
  const [listingStatus, setListingStatus] = useState('');
  // Add state for user deletion modal
  const [showDeleteUserModal, setShowDeleteUserModal] = useState(false);
  const [userToDelete, setUserToDelete] = useState(null);

  // --- Data Fetching & Side Effects ---
  useEffect(() => {
    if (isAuthenticated && isAdmin) {
      fetchAdminData();
    }
  }, [isAuthenticated, isAdmin]);

  // Listen for listing updates and auto-refresh
  useEffect(() => {
    const handleStorageChange = (e) => {
      if (e.key === 'listingsUpdated') {
        fetchAdminData();
      }
    };

    // Auto-refresh every 30 seconds
    const interval = setInterval(() => {
      fetchAdminData();
    }, 30000);

    window.addEventListener('storage', handleStorageChange);
    return () => {
      window.removeEventListener('storage', handleStorageChange);
      clearInterval(interval);
    };
  }, []);

  const fetchAdminData = async () => {
    try {
      setLoading(true);
      const [statsResponse, usersResponse, listingsResponse, dealershipsResponse, pendingApprovalsResponse, activityLogsResponse] = await Promise.all([
        apiService.getAdminStats(),
        apiService.getUsers(),
        apiService.getAdminListings(),
        apiService.getAdminDealerships(),
        apiService.getPendingApprovals(),
        apiService.getRecentActivity()
      ]);
      setStats(statsResponse);
      setUsers(usersResponse);
      setListings(listingsResponse);
      setDealerships(dealershipsResponse);
      setPendingApprovals(pendingApprovalsResponse);
      setActivityLogs(activityLogsResponse);
    } catch (error) {
      console.error('❌ [Dashboard] Error fetching admin data:', error);
    } finally {
      setLoading(false);
    }
  };

  // --- Helper Functions ---
  const getActivityIcon = (activityType) => {
    switch (activityType) {
      case 1: // UserRegistration
      case 7: // UserLogin
        return (
          <svg viewBox="0 0 24 24" fill="currentColor">
            <path d="M12,4A4,4 0 0,1 16,8A4,4 0 0,1 12,12A4,4 0 0,1 8,8A4,4 0 0,1 12,4M12,14C16.42,14 20,15.79 20,18V20H4V18C4,15.79 7.58,14 12,14Z" />
          </svg>
        );
      case 2: // ListingCreated
        return (
          <svg viewBox="0 0 24 24" fill="currentColor">
            <path d="M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z" />
          </svg>
        );
      case 3: // ListingApproved
        return (
          <svg viewBox="0 0 24 24" fill="currentColor">
            <path d="M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M11,16.5L18,9.5L16.59,8.09L11,13.67L7.91,10.59L6.5,12L11,16.5Z" />
          </svg>
        );
      case 4: // ListingRejected
      case 6: // ListingFlagged
        return (
          <svg viewBox="0 0 24 24" fill="currentColor">
            <path d="M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z" />
          </svg>
        );
      case 5: // DealershipCreated
        return (
          <svg viewBox="0 0 24 24" fill="currentColor">
            <path d="M12,2L3,9H6V20H18V9H21L12,2M9,12H15V10.5A1.5,1.5 0 0,0 13.5,9A1.5,1.5 0 0,0 12,10.5A1.5,1.5 0 0,0 10.5,9A1.5,1.5 0 0,0 9,10.5V12Z" />
          </svg>
        );
      default:
        return (
          <svg viewBox="0 0 24 24" fill="currentColor">
            <path d="M13,9H11V7H13M13,17H11V11H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z" />
          </svg>
        );
    }
  };

  const getListingsCountForDealership = (dealershipId) => {
    return listings.filter(listing =>
      listing.dealership && listing.dealership.id === dealershipId
    ).length;
  };

  // --- Dealerships based on users with role 1 ---
  const dealershipUsers = users.filter(u => u.role === 1 || u.role === 'Dealership');
  const totalDealerships = dealershipUsers.length;

  // Prepare a merged list for the dealership section
  const dealershipRows = dealershipUsers.map(user => {
    // Try to find a matching dealership record by userId
    const dealership = dealerships.find(d => d.userId === user.id);
    return {
      id: user.id,
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      role: user.role,
      // If dealership exists, use its info; else fallback to user info
      companyName: dealership ? dealership.name : undefined,
      location: dealership ? dealership.location : undefined,
      companyUniqueNumber: dealership ? dealership.companyUniqueNumber : undefined,
      listingsCount: dealership ? getListingsCountForDealership(dealership.id) : 0,
      hasDealershipProfile: !!dealership
    };
  });

  // --- Event Handlers ---
  const handleApproveListing = async (listingId) => {
    try {
      await apiService.approveListing(listingId);
      const [pendingApprovalsResponse, statsResponse] = await Promise.all([
        apiService.getPendingApprovals(),
        apiService.getAdminStats()
      ]);
      setPendingApprovals(pendingApprovalsResponse);
      setStats(statsResponse);
      localStorage.setItem('listingsUpdated', Date.now().toString());
    } catch (error) {
      console.error('Error approving listing:', error);
    }
  };

  const handleRejectListing = async (listingId) => {
    try {
      await apiService.rejectListing(listingId);
      const [pendingApprovalsResponse, statsResponse] = await Promise.all([
        apiService.getPendingApprovals(),
        apiService.getAdminStats()
      ]);
      setPendingApprovals(pendingApprovalsResponse);
      setStats(statsResponse);
      localStorage.setItem('listingsUpdated', Date.now().toString());
    } catch (error) {
      console.error('Error rejecting listing:', error);
    }
  };

  const handleViewListing = (listingId) => {
    navigate(`/listings/${listingId}`);
  };

  const handleEditListing = (listingId) => {
    navigate(`/edit-listing/${listingId}`);
  };

  const handleRemoveListing = (listingId) => {
    setListingToDelete(listingId);
    setShowDeleteModal(true);
  };

  const confirmDeleteListing = async () => {
    try {
      await apiService.deleteCarListing(listingToDelete);
      setListings(listings.filter(listing => listing.id !== listingToDelete));
      const statsResponse = await apiService.getAdminStats();
      setStats(statsResponse);
      localStorage.setItem('listingsUpdated', Date.now().toString());
    } catch (error) {
      console.error('Error deleting listing:', error);
      alert('Failed to delete listing. Please try again.');
    } finally {
      setShowDeleteModal(false);
      setListingToDelete(null);
    }
  };

  // Add user deletion handler
  const handleDeleteUser = (userId) => {
    setUserToDelete(userId);
    setShowDeleteUserModal(true);
  };

  // Add confirmDeleteUser function
  const confirmDeleteUser = async () => {
    try {
      // Delete all listings for the user first
      await apiService.deleteAllListingsByUser(userToDelete);
      // Then delete the user
      await apiService.deleteUser(userToDelete);
      setUsers(users.filter(u => u.id !== userToDelete));
      setShowDeleteUserModal(false);
      setUserToDelete(null);
      // Optionally, refresh listings if needed
      fetchAdminData();
    } catch (err) {
      alert('Failed to delete user and/or their listings.');
    }
  };

  // --- Conditional Renders ---
  if (!isAuthenticated) {
    return (
      <div className="container mt-5 text-center">
        <h2>Please log in to access the dashboard</h2>
        <a href="/login" className="btn btn-primary mt-3">Login</a>
      </div>
    );
  }

  if (!isAdmin) {
    return (
      <div className="container mt-5 text-center">
        <h2>Access Denied</h2>
        <p>You don't have permission to access the dashboard.</p>
        <a href="/" className="btn btn-primary mt-3">Go Home</a>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="container mt-5 text-center">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
        <p className="mt-2">Loading admin data...</p>
      </div>
    );
  }

  // Filtered users
  const filteredUsers = users.filter(user => {
    const matchesSearch =
      user.firstName?.toLowerCase().includes(userSearch.toLowerCase()) ||
      user.lastName?.toLowerCase().includes(userSearch.toLowerCase()) ||
      user.email?.toLowerCase().includes(userSearch.toLowerCase());
    const matchesRole = userRole ? user.role === userRole : true;
    return matchesSearch && matchesRole;
  });
  // Filtered listings
  const filteredListings = listings.filter(listing => {
    const matchesSearch =
      listing.title?.toLowerCase().includes(listingSearch.toLowerCase()) ||
      listing.make?.toLowerCase().includes(listingSearch.toLowerCase()) ||
      listing.model?.toLowerCase().includes(listingSearch.toLowerCase());
    const matchesStatus = listingStatus ? listing.status === listingStatus : true;
    return matchesSearch && matchesStatus;
  });

  // --- Render Functions for Tabs ---
  const renderOverview = () => (
    <div>
      {/* Stats Row */}
      <div style={{ display: 'flex', gap: '18px', flexWrap: 'wrap', marginBottom: 24 }}>
        <div style={{ flex: 1, minWidth: 160 }}>
          <div style={{ background: '#f2f4f8', borderRadius: 12, padding: 18, textAlign: 'center' }}>
            <div style={{ fontSize: 32, fontWeight: 700, color: '#2563eb' }}>{stats.totalUsers.toLocaleString()}</div>
            <div style={{ color: '#3a4256', fontWeight: 500 }}>Total Users</div>
          </div>
        </div>
        <div style={{ flex: 1, minWidth: 160 }}>
          <div style={{ background: '#f2f4f8', borderRadius: 12, padding: 18, textAlign: 'center' }}>
            <div style={{ fontSize: 32, fontWeight: 700, color: '#2563eb' }}>{stats.totalListings.toLocaleString()}</div>
            <div style={{ color: '#3a4256', fontWeight: 500 }}>Total Listings</div>
          </div>
        </div>
        <div style={{ flex: 1, minWidth: 160 }}>
          <div style={{ background: '#f2f4f8', borderRadius: 12, padding: 18, textAlign: 'center' }}>
            <div style={{ fontSize: 32, fontWeight: 700, color: '#2563eb' }}>{totalDealerships}</div>
            <div style={{ color: '#3a4256', fontWeight: 500 }}>Dealerships</div>
          </div>
        </div>
        <div style={{ flex: 1, minWidth: 160 }}>
          <div style={{ background: '#f2f4f8', borderRadius: 12, padding: 18, textAlign: 'center' }}>
            <div style={{ fontSize: 32, fontWeight: 700, color: '#e11d48' }}>{stats.pendingApprovals}</div>
            <div style={{ color: '#3a4256', fontWeight: 500 }}>Pending Approvals</div>
          </div>
        </div>
        <div style={{ flex: 1, minWidth: 160 }}>
          <div style={{ background: '#f2f4f8', borderRadius: 12, padding: 18, textAlign: 'center' }}>
            <div style={{ fontSize: 32, fontWeight: 700, color: '#16a34a' }}>${stats.monthlyRevenue.toLocaleString()}</div>
            <div style={{ color: '#3a4256', fontWeight: 500 }}>Monthly Revenue</div>
          </div>
        </div>
        <div style={{ flex: 1, minWidth: 160 }}>
          <div style={{ background: '#f2f4f8', borderRadius: 12, padding: 18, textAlign: 'center' }}>
            <div style={{ fontSize: 32, fontWeight: 700, color: '#2563eb' }}>{stats.activeUsers}</div>
            <div style={{ color: '#3a4256', fontWeight: 500 }}>Active Users</div>
          </div>
        </div>
      </div>
      {/* Recent Activity */}
      <div style={{ display: 'flex', gap: 24, flexWrap: 'wrap' }}>
        <div style={{ flex: 2, minWidth: 320, background: '#fafbfc', borderRadius: 12, boxShadow: '0 1px 4px rgba(0,0,0,0.03)', padding: 24 }}>
          <div style={{ fontWeight: 600, fontSize: 18, marginBottom: 12 }}>Recent Activity</div>
          <div>
            {activityLogs.length > 0 ? (
              activityLogs.map(activity => (
                <div key={activity.id} style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 14 }}>
                  <div style={{ width: 36, height: 36, borderRadius: '50%', background: '#eaf1ff', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    {getActivityIcon(activity.type)}
                  </div>
                  <div>
                    <div style={{ fontWeight: 500 }}>{activity.typeName}: {activity.message}</div>
                    <div style={{ fontSize: 12, color: '#888' }}>{activity.timeAgo}</div>
                  </div>
                </div>
              ))
            ) : (
              <div style={{ color: '#888' }}>No recent activity found</div>
            )}
          </div>
        </div>
        <div style={{ flex: 1, minWidth: 220, background: '#fafbfc', borderRadius: 12, boxShadow: '0 1px 4px rgba(0,0,0,0.03)', padding: 24 }}>
          <div style={{ fontWeight: 600, fontSize: 18, marginBottom: 12 }}>Pending Approvals</div>
          <div>
            {pendingApprovals.length > 0 ? (
              pendingApprovals.map(item => (
                <div key={item.id} style={{ marginBottom: 16, borderBottom: '1px solid #e5e7eb', paddingBottom: 10 }}>
                  <div style={{ fontWeight: 500 }}>{item.title || 'Untitled'}</div>
                  <div style={{ fontSize: 13, color: '#888' }}>{item.type || 'Unknown'} • {item.submitter || 'Unknown'}</div>
                  <div style={{ fontSize: 12, color: '#aaa' }}>{item.date ? new Date(item.date).toLocaleDateString() : 'No date'}</div>
                  <div style={{ marginTop: 6 }}>
                    <button className="admin-dashboard-btn" style={{ padding: '4px 12px', fontSize: 13, marginRight: 6 }} onClick={() => handleApproveListing(item.id)}>Approve</button>
                    <button className="admin-dashboard-btn delete" style={{ padding: '4px 12px', fontSize: 13 }} onClick={() => handleRejectListing(item.id)}>Reject</button>
                  </div>
                </div>
              ))
            ) : (
              <div style={{ color: '#888' }}>No pending approvals</div>
            )}
          </div>
        </div>
      </div>
    </div>
  );

  const renderUsers = () => (
    <div>
      <div className="admin-dashboard-filters" style={{ marginBottom: 18 }}>
        <input
          type="text"
          placeholder="Search users..."
          value={userSearch}
          onChange={e => setUserSearch(e.target.value)}
        />
        <select
          value={userRole}
          onChange={e => setUserRole(e.target.value)}
        >
          <option value="">All Roles</option>
          <option value="User">User</option>
          <option value="Dealer">Dealer</option>
          <option value="Admin">Admin</option>
        </select>
        <button className="admin-dashboard-btn">Export</button>
      </div>
      <div className="admin-dashboard-table-container">
        <table className="admin-dashboard-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th>Joined</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredUsers.map(user => (
              <tr key={user.id}>
                <td>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                    <div style={{ width: 36, height: 36, borderRadius: '50%', background: '#eaf1ff', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 600, color: '#2563eb' }}>
                      {user.firstName?.charAt(0) || user.email?.charAt(0) || 'U'}
                    </div>
                    <div>
                      <div style={{ fontWeight: 500 }}>{user.firstName} {user.lastName}</div>
                      <div style={{ fontSize: 12, color: '#888' }}>{user.id}</div>
                    </div>
                  </div>
                </td>
                <td>{user.email}</td>
                <td>
                  <span style={{ background: '#f2f4f8', color: '#2563eb', borderRadius: 6, padding: '2px 10px', fontWeight: 500, fontSize: 13 }}>{user.role || 'User'}</span>
                </td>
                <td>
                  <span style={{ background: user.isActive ? '#d1fae5' : '#fee2e2', color: user.isActive ? '#16a34a' : '#e11d48', borderRadius: 6, padding: '2px 10px', fontWeight: 500, fontSize: 13 }}>{user.isActive ? 'Active' : 'Inactive'}</span>
                </td>
                <td>{user.createdAt ? new Date(user.createdAt).toLocaleDateString() : 'N/A'}</td>
                <td>
                  <button className="admin-dashboard-btn" style={{ padding: '4px 12px', fontSize: 13, marginRight: 6 }}>View</button>
                  <button className="admin-dashboard-btn" style={{ padding: '4px 12px', fontSize: 13, marginRight: 6 }}>Edit</button>
                  <button className="admin-dashboard-btn delete" style={{ padding: '4px 12px', fontSize: 13 }} onClick={() => handleDeleteUser(user.id)}>Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );

  const renderDealerships = () => (
    <div>
      <div className="admin-dashboard-filters" style={{ marginBottom: 18 }}>
        <input
          type="text"
          placeholder="Search dealerships..."
        />
        <button className="admin-dashboard-btn">Export</button>
      </div>
      <div className="admin-dashboard-table-container">
        <table className="admin-dashboard-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Status</th>
              <th>Joined</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {dealershipRows.map(dealership => (
              <tr key={dealership.id}>
                <td>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                    <div style={{ width: 36, height: 36, borderRadius: '50%', background: '#eaf1ff', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 600, color: '#2563eb' }}>
                      {(dealership.companyName || dealership.firstName || dealership.email || 'U').charAt(0)}
                    </div>
                    <div>
                      <div style={{ fontWeight: 500 }}>{dealership.companyName || (dealership.firstName + ' ' + dealership.lastName) || 'N/A'}</div>
                      <div style={{ fontSize: 12, color: '#888' }}>{dealership.id}</div>
                    </div>
                  </div>
                </td>
                <td>{dealership.email || 'N/A'}</td>
                <td>
                  <span style={{ background: '#d1fae5', color: '#16a34a', borderRadius: 6, padding: '2px 10px', fontWeight: 500, fontSize: 13 }}>
                    Active
                  </span>
                </td>
                <td>{dealership.createdAt ? new Date(dealership.createdAt).toLocaleDateString() : 'N/A'}</td>
                <td>
                  <button className="admin-dashboard-btn" style={{ padding: '4px 12px', fontSize: 13, marginRight: 6 }}>View</button>
                  <button className="admin-dashboard-btn" style={{ padding: '4px 12px', fontSize: 13, marginRight: 6 }}>Edit</button>
                  <button className="admin-dashboard-btn delete" style={{ padding: '4px 12px', fontSize: 13 }}>Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );

  const renderListings = () => (
    <div>
      <div className="admin-dashboard-filters" style={{ marginBottom: 18 }}>
        <input
          type="text"
          placeholder="Search listings..."
          value={listingSearch}
          onChange={e => setListingSearch(e.target.value)}
        />
        <select
          value={listingStatus}
          onChange={e => setListingStatus(e.target.value)}
        >
          <option value="">All Status</option>
          <option value="Active">Active</option>
          <option value="Pending">Pending</option>
          <option value="Sold">Sold</option>
          <option value="Rejected">Rejected</option>
        </select>
        <button className="admin-dashboard-btn">Export</button>
      </div>
      <div className="admin-dashboard-table-container">
        <table className="admin-dashboard-table">
          <thead>
            <tr>
              <th>Vehicle</th>
              <th>Price</th>
              <th>Status</th>
              <th>Owner</th>
              <th>Created</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredListings.map(listing => (
              <tr
                key={listing.id}
                style={{ cursor: 'pointer' }}
                onClick={() => handleViewListing(listing.id)}
              >
                <td>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                    <div style={{ width: 50, height: 50, borderRadius: 8, overflow: 'hidden', background: '#f2f4f8', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                      <img
                        src={listing.primaryImageUrl || listing.images?.[0]?.imageUrl || '/api/placeholder/50/50'}
                        alt={listing.title}
                        style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                      />
                    </div>
                    <div>
                      <div style={{ fontWeight: 500 }}>{listing.title || `${listing.year} ${listing.make} ${listing.model}`}</div>
                      <div style={{ fontSize: 12, color: '#888' }}>
                        {listing.year} {listing.make} {listing.model} • {listing.mileage?.toLocaleString()} km
                      </div>
                    </div>
                  </div>
                </td>
                <td>
                  <span style={{ fontWeight: 600, color: '#16a34a' }}>${listing.price?.toLocaleString()}</span>
                </td>
                <td>
                  <span style={{ background: '#f2f4f8', color: '#2563eb', borderRadius: 6, padding: '2px 10px', fontWeight: 500, fontSize: 13 }}>{listing.status || 'Unknown'}</span>
                </td>
                <td>
                  <div>
                    <div style={{ fontWeight: 500 }}>{listing.ownerName || 'N/A'}</div>
                    <div style={{ fontSize: 12, color: '#888' }}>{listing.ownerEmail || 'N/A'}</div>
                  </div>
                </td>
                <td>{listing.CreatedAt ? new Date(listing.CreatedAt).toLocaleDateString() : 'N/A'}</td>
                <td>
                  <button className="admin-dashboard-btn" style={{ padding: '4px 12px', fontSize: 13, marginRight: 6 }} onClick={e => { e.stopPropagation(); handleViewListing(listing.id); }}>View</button>
                  <button className="admin-dashboard-btn" style={{ padding: '4px 12px', fontSize: 13, marginRight: 6 }} onClick={e => { e.stopPropagation(); handleEditListing(listing.id); }}>Edit</button>
                  <button className="admin-dashboard-btn delete" style={{ padding: '4px 12px', fontSize: 13 }} onClick={e => { e.stopPropagation(); handleRemoveListing(listing.id); }}>Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );

  const renderContent = () => {
    switch (activeTab) {
      case 'overview':
        return renderOverview();
      case 'users':
        return renderUsers();
      case 'listings':
        return renderListings();
      case 'dealerships':
        return renderDealerships();
      default:
        return renderOverview();
    }
  };

  return (
    <div style={{ display: 'flex', gap: 24, padding: '40px 24px' }}>
      {/* Sidebar Navigation - Outside the main content */}
      <div style={{
        width: 200,
        background: '#f8fafc',
        borderRadius: 12,
        padding: 16,
        height: 'fit-content',
        position: 'sticky',
        top: 24
      }}>
        <div style={{
          display: 'flex',
          flexDirection: 'column',
          gap: 8
        }}>
          <button
            className={`admin-dashboard-btn ${activeTab === 'overview' ? '' : ''}`}
            style={{
              justifyContent: 'flex-start',
              textAlign: 'left',
              background: activeTab === 'overview' ? 'linear-gradient(90deg, #2563eb 0%, #4f8cff 100%)' : 'transparent',
              color: activeTab === 'overview' ? '#fff' : '#3a4256',
              border: activeTab === 'overview' ? 'none' : '1px solid #e5e7eb',
              fontWeight: activeTab === 'overview' ? 600 : 500
            }}
            onClick={() => setActiveTab('overview')}
          >
            Overview
          </button>
          <button
            className={`admin-dashboard-btn ${activeTab === 'users' ? '' : ''}`}
            style={{
              justifyContent: 'flex-start',
              textAlign: 'left',
              background: activeTab === 'users' ? 'linear-gradient(90deg, #2563eb 0%, #4f8cff 100%)' : 'transparent',
              color: activeTab === 'users' ? '#fff' : '#3a4256',
              border: activeTab === 'users' ? 'none' : '1px solid #e5e7eb',
              fontWeight: activeTab === 'users' ? 600 : 500
            }}
            onClick={() => setActiveTab('users')}
          >
            Users
          </button>
          <button
            className={`admin-dashboard-btn ${activeTab === 'listings' ? '' : ''}`}
            style={{
              justifyContent: 'flex-start',
              textAlign: 'left',
              background: activeTab === 'listings' ? 'linear-gradient(90deg, #2563eb 0%, #4f8cff 100%)' : 'transparent',
              color: activeTab === 'listings' ? '#fff' : '#3a4256',
              border: activeTab === 'listings' ? 'none' : '1px solid #e5e7eb',
              fontWeight: activeTab === 'listings' ? 600 : 500
            }}
            onClick={() => setActiveTab('listings')}
          >
            Listings
          </button>
          <button
            className={`admin-dashboard-btn ${activeTab === 'dealerships' ? '' : ''}`}
            style={{
              justifyContent: 'flex-start',
              textAlign: 'left',
              background: activeTab === 'dealerships' ? 'linear-gradient(90deg, #2563eb 0%, #4f8cff 100%)' : 'transparent',
              color: activeTab === 'dealerships' ? '#fff' : '#3a4256',
              border: activeTab === 'dealerships' ? 'none' : '1px solid #e5e7eb',
              fontWeight: activeTab === 'dealerships' ? 600 : 500
            }}
            onClick={() => setActiveTab('dealerships')}
          >
            Dealerships
          </button>
        </div>
      </div>

      {/* Main Content Area - Separate from sidebar */}
      <div className="admin-dashboard-container" style={{ flex: 1, minWidth: 0 }}>
        <div className="admin-dashboard-title">Admin Dashboard</div>
        <div style={{ marginTop: 24 }}>
          {renderContent()}
        </div>
      </div>

      {/* Delete Confirmation Modal */}
      {showDeleteModal && (
        <div className="modal-overlay">
          <div className="modal-content">
            <h3>Confirm Deletion</h3>
            <p>Are you sure you want to delete this listing? This action cannot be undone.</p>
            <div className="modal-actions">
              <button className="admin-dashboard-btn delete" onClick={confirmDeleteListing}>Delete</button>
              <button className="admin-dashboard-btn" onClick={() => setShowDeleteModal(false)}>Cancel</button>
            </div>
          </div>
        </div>
      )}
      {showDeleteUserModal && (
        <div className="modal-overlay">
          <div className="modal-content">
            <h3>Confirm Deletion</h3>
            <p style={{ color: '#e11d48', fontWeight: 500 }}>
              Are you sure you want to delete this user?<br />
              <b>This will also delete all of their listings, dealership profile, and saved listings. This action cannot be undone.</b>
            </p>
            <div className="modal-actions">
              <button className="admin-dashboard-btn delete" onClick={confirmDeleteUser}>Delete</button>
              <button className="admin-dashboard-btn" onClick={() => setShowDeleteUserModal(false)}>Cancel</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Dashboard;
