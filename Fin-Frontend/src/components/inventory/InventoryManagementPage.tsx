import React, { useState } from 'react';
import { motion } from 'framer-motion';
import {
  Package,
  Plus,
  Search,
  Filter,
  Download,
  Eye,
  MoreHorizontal,
  AlertTriangle,
  TrendingDown,
  TrendingUp,
  Minus,
  BarChart3,
  ShoppingCart,
  Truck,
  Archive,
} from 'lucide-react';

interface InventoryItem {
  id: string;
  itemCode: string;
  itemName: string;
  category: string;
  currentStock: number;
  reorderLevel: number;
  maximumLevel: number;
  unitCost: number;
  sellingPrice: number;
  totalValue: number;
  lastUpdated: string;
  status: 'In Stock' | 'Low Stock' | 'Out of Stock' | 'Overstock';
  supplier: string;
  location: string;
}

const InventoryManagementPage: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [filterCategory, setFilterCategory] = useState('all');
  const [filterStatus, setFilterStatus] = useState('all');
  const [selectedView, setSelectedView] = useState<'list' | 'cards'>('list');

  // Mock data - replace with actual API call
  const inventoryItems: InventoryItem[] = [
    {
      id: '1',
      itemCode: 'ITM001',
      itemName: 'Samsung Galaxy A54 5G',
      category: 'Electronics',
      currentStock: 25,
      reorderLevel: 10,
      maximumLevel: 50,
      unitCost: 180000,
      sellingPrice: 220000,
      totalValue: 4500000,
      lastUpdated: '2024-12-20',
      status: 'In Stock',
      supplier: 'Samsung Nigeria',
      location: 'Warehouse A',
    },
    {
      id: '2',
      itemCode: 'ITM002',
      itemName: 'Nike Air Force 1',
      category: 'Footwear',
      currentStock: 8,
      reorderLevel: 15,
      maximumLevel: 40,
      unitCost: 45000,
      sellingPrice: 65000,
      totalValue: 360000,
      lastUpdated: '2024-12-19',
      status: 'Low Stock',
      supplier: 'Nike Distributors',
      location: 'Store Front',
    },
    {
      id: '3',
      itemCode: 'ITM003',
      itemName: 'HP Pavilion Laptop',
      category: 'Electronics',
      currentStock: 0,
      reorderLevel: 5,
      maximumLevel: 20,
      unitCost: 350000,
      sellingPrice: 450000,
      totalValue: 0,
      lastUpdated: '2024-12-18',
      status: 'Out of Stock',
      supplier: 'HP Nigeria',
      location: 'Warehouse B',
    },
    {
      id: '4',
      itemCode: 'ITM004',
      itemName: 'Adidas Tracksuit',
      category: 'Clothing',
      currentStock: 45,
      reorderLevel: 20,
      maximumLevel: 30,
      unitCost: 25000,
      sellingPrice: 35000,
      totalValue: 1125000,
      lastUpdated: '2024-12-20',
      status: 'Overstock',
      supplier: 'Adidas Store',
      location: 'Store Front',
    },
    {
      id: '5',
      itemCode: 'ITM005',
      itemName: 'iPhone 15 Pro',
      category: 'Electronics',
      currentStock: 12,
      reorderLevel: 8,
      maximumLevel: 25,
      unitCost: 650000,
      sellingPrice: 800000,
      totalValue: 7800000,
      lastUpdated: '2024-12-21',
      status: 'In Stock',
      supplier: 'Apple Authorized',
      location: 'Secure Storage',
    },
  ];

  const filteredItems = inventoryItems.filter((item) => {
    const matchesSearch = item.itemName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         item.itemCode.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         item.supplier.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesCategory = filterCategory === 'all' || item.category.toLowerCase() === filterCategory;
    const matchesStatus = filterStatus === 'all' || item.status.toLowerCase().replace(' ', '') === filterStatus;
    
    return matchesSearch && matchesCategory && matchesStatus;
  });

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'In Stock':
        return 'bg-green-100 text-green-800';
      case 'Low Stock':
        return 'bg-yellow-100 text-yellow-800';
      case 'Out of Stock':
        return 'bg-red-100 text-red-800';
      case 'Overstock':
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'In Stock':
        return <TrendingUp className="w-4 h-4" />;
      case 'Low Stock':
        return <AlertTriangle className="w-4 h-4" />;
      case 'Out of Stock':
        return <TrendingDown className="w-4 h-4" />;
      case 'Overstock':
        return <Archive className="w-4 h-4" />;
      default:
        return <Minus className="w-4 h-4" />;
    }
  };

  const totalItems = inventoryItems.length;
  const totalValue = inventoryItems.reduce((sum, item) => sum + item.totalValue, 0);
  const lowStockItems = inventoryItems.filter(item => item.status === 'Low Stock').length;
  const outOfStockItems = inventoryItems.filter(item => item.status === 'Out of Stock').length;
  const overstockItems = inventoryItems.filter(item => item.status === 'Overstock').length;

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center">
            <Package className="w-7 h-7 text-emerald-600 mr-3" />
            Inventory Management System
          </h1>
          <p className="text-gray-600">Real-time stock control with automated reorder alerts</p>
        </div>
        <div className="flex items-center space-x-3">
          <button className="flex items-center px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
            <Download className="w-4 h-4 mr-2" />
            Export Inventory
          </button>
          <button className="flex items-center px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors">
            <Plus className="w-4 h-4 mr-2" />
            Add New Item
          </button>
        </div>
      </div>

      {/* Inventory Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Total Items</p>
              <p className="text-2xl font-bold text-gray-900">{totalItems}</p>
              <div className="flex items-center mt-2">
                <TrendingUp className="w-4 h-4 text-green-500 mr-1" />
                <span className="text-sm text-green-600 font-medium">+8.2%</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-emerald-100 text-emerald-600">
              <Package className="w-6 h-6" />
            </div>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Total Value</p>
              <p className="text-2xl font-bold text-gray-900">₦{totalValue.toLocaleString()}</p>
              <div className="flex items-center mt-2">
                <TrendingUp className="w-4 h-4 text-blue-500 mr-1" />
                <span className="text-sm text-blue-600 font-medium">+15.3%</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-blue-100 text-blue-600">
              <BarChart3 className="w-6 h-6" />
            </div>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Low Stock Items</p>
              <p className="text-2xl font-bold text-gray-900">{lowStockItems}</p>
              <div className="flex items-center mt-2">
                <AlertTriangle className="w-4 h-4 text-yellow-500 mr-1" />
                <span className="text-sm text-yellow-600 font-medium">Needs Attention</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-yellow-100 text-yellow-600">
              <AlertTriangle className="w-6 h-6" />
            </div>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Out of Stock</p>
              <p className="text-2xl font-bold text-gray-900">{outOfStockItems}</p>
              <div className="flex items-center mt-2">
                <TrendingDown className="w-4 h-4 text-red-500 mr-1" />
                <span className="text-sm text-red-600 font-medium">Critical</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-red-100 text-red-600">
              <TrendingDown className="w-6 h-6" />
            </div>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Overstock Items</p>
              <p className="text-2xl font-bold text-gray-900">{overstockItems}</p>
              <div className="flex items-center mt-2">
                <Archive className="w-4 h-4 text-purple-500 mr-1" />
                <span className="text-sm text-purple-600 font-medium">Review Needed</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-purple-100 text-purple-600">
              <Archive className="w-6 h-6" />
            </div>
          </div>
        </motion.div>
      </div>

      {/* Filters and Search */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
        <div className="flex flex-col lg:flex-row lg:items-center space-y-4 lg:space-y-0 lg:space-x-4">
          {/* Search */}
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="Search by item name, code, or supplier..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
            />
          </div>

          {/* View Toggle */}
          <div className="flex items-center bg-gray-100 rounded-lg p-1">
            <button
              onClick={() => setSelectedView('list')}
              className={`px-3 py-1 rounded-md text-sm font-medium transition-colors ${
                selectedView === 'list'
                  ? 'bg-white text-gray-900 shadow-sm'
                  : 'text-gray-600 hover:text-gray-900'
              }`}
            >
              List View
            </button>
            <button
              onClick={() => setSelectedView('cards')}
              className={`px-3 py-1 rounded-md text-sm font-medium transition-colors ${
                selectedView === 'cards'
                  ? 'bg-white text-gray-900 shadow-sm'
                  : 'text-gray-600 hover:text-gray-900'
              }`}
            >
              Card View
            </button>
          </div>

          {/* Category Filter */}
          <div className="flex items-center space-x-2">
            <Filter className="w-5 h-5 text-gray-400" />
            <select
              value={filterCategory}
              onChange={(e) => setFilterCategory(e.target.value)}
              className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
            >
              <option value="all">All Categories</option>
              <option value="electronics">Electronics</option>
              <option value="clothing">Clothing</option>
              <option value="footwear">Footwear</option>
              <option value="accessories">Accessories</option>
            </select>
          </div>

          {/* Status Filter */}
          <div className="flex items-center space-x-2">
            <select
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
              className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
            >
              <option value="all">All Status</option>
              <option value="instock">In Stock</option>
              <option value="lowstock">Low Stock</option>
              <option value="outofstock">Out of Stock</option>
              <option value="overstock">Overstock</option>
            </select>
          </div>
        </div>
      </div>

      {/* Inventory Display */}
      {selectedView === 'list' ? (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200">
            <h3 className="text-lg font-semibold text-gray-900">
              Inventory Items ({filteredItems.length})
            </h3>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Item Details
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Category
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Stock Level
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Unit Cost
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Selling Price
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Total Value
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredItems.map((item, index) => (
                  <motion.tr
                    key={item.id}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: index * 0.05 }}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div>
                        <div className="text-sm font-medium text-gray-900">
                          {item.itemName}
                        </div>
                        <div className="text-sm text-gray-500">
                          Code: {item.itemCode} • {item.supplier}
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {item.category}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">
                        {item.currentStock} units
                      </div>
                      <div className="text-sm text-gray-500">
                        Reorder: {item.reorderLevel} • Max: {item.maximumLevel}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      ₦{item.unitCost.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      ₦{item.sellingPrice.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      ₦{item.totalValue.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full ${
                        getStatusColor(item.status)
                      }`}>
                        {getStatusIcon(item.status)}
                        <span className="ml-1">{item.status}</span>
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                      <div className="flex items-center space-x-2">
                        <button className="text-emerald-600 hover:text-emerald-900 transition-colors">
                          <Eye className="w-4 h-4" />
                        </button>
                        <button className="text-gray-400 hover:text-gray-600 transition-colors">
                          <MoreHorizontal className="w-4 h-4" />
                        </button>
                      </div>
                    </td>
                  </motion.tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredItems.map((item, index) => (
            <motion.div
              key={item.id}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.1 }}
              className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow"
            >
              <div className="flex items-start justify-between mb-4">
                <div>
                  <h3 className="text-lg font-semibold text-gray-900">{item.itemName}</h3>
                  <p className="text-sm text-gray-600">{item.itemCode} • {item.category}</p>
                </div>
                <span className={`inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full ${
                  getStatusColor(item.status)
                }`}>
                  {getStatusIcon(item.status)}
                  <span className="ml-1">{item.status}</span>
                </span>
              </div>

              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Current Stock:</span>
                  <span className="text-sm font-medium text-gray-900">{item.currentStock} units</span>
                </div>

                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Unit Cost:</span>
                  <span className="text-sm font-medium text-gray-900">₦{item.unitCost.toLocaleString()}</span>
                </div>

                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Selling Price:</span>
                  <span className="text-sm font-medium text-gray-900">₦{item.sellingPrice.toLocaleString()}</span>
                </div>

                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Total Value:</span>
                  <span className="text-sm font-bold text-emerald-600">₦{item.totalValue.toLocaleString()}</span>
                </div>

                <div className="pt-2 border-t border-gray-200">
                  <div className="flex items-center justify-between text-xs text-gray-500">
                    <span>Supplier: {item.supplier}</span>
                    <span>Location: {item.location}</span>
                  </div>
                </div>
              </div>

              <div className="mt-6 flex items-center justify-between">
                <button className="flex items-center px-3 py-2 text-sm font-medium text-emerald-600 hover:text-emerald-700 transition-colors">
                  <Eye className="w-4 h-4 mr-1" />
                  View Details
                </button>
                <button className="p-2 text-gray-400 hover:text-gray-600 transition-colors">
                  <MoreHorizontal className="w-4 h-4" />
                </button>
              </div>
            </motion.div>
          ))}
        </div>
      )}

      {filteredItems.length === 0 && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 px-6 py-12 text-center">
          <Package className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">No items found</h3>
          <p className="text-gray-500">
            {searchTerm || filterCategory !== 'all' || filterStatus !== 'all'
              ? 'Try adjusting your search or filter criteria.'
              : 'Get started by adding your first inventory item.'
            }
          </p>
        </div>
      )}

      {/* Quick Actions Panel */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.5 }}
        className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
      >
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Inventory Management Actions</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Plus className="w-6 h-6 text-emerald-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Add New Item</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <ShoppingCart className="w-6 h-6 text-blue-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Purchase Order</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Truck className="w-6 h-6 text-green-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Receive Stock</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <BarChart3 className="w-6 h-6 text-purple-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Stock Report</span>
          </button>
        </div>
      </motion.div>
    </div>
  );
};

export default InventoryManagementPage;