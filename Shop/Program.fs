namespace Shop

open System
open Shop.Models
open Shop.DataAccess
open Shop.Business

module Program =
    
    // Load data from JSON files (async) with parallel loading
    let loadDataAsync () : Async<Result<Product list * Customer list, ShopError>> =
        async {
            // Load both datasets in parallel
            let! productsTask = ProductRepository.loadProductsAsync() |> Async.StartChild
            let! customersTask = CustomerRepository.loadCustomersAsync() |> Async.StartChild
            
            let! productsResult = productsTask
            let! customersResult = customersTask
            
            match productsResult, customersResult with
            | Ok products, Ok customers -> return Ok (products, customers)
            | Error err, _ -> return Error err
            | _, Error err -> return Error err
        }

    
    // Display functions
    let displayProducts (products: Product list) =
        printfn ""
        printfn "📋 DANH SÁCH SẢN PHẨM CÓ SẴN"
        printfn "================================="
        products
        |> List.iteri (fun i product -> 
            printfn $"[{i + 1}] {product.Name}"
            printfn $"    💰 Giá: ${product.Price:F2}"
            printfn $"    📦 Tồn kho: {product.Stock} sản phẩm"
            printfn $"    🏷️  Danh mục: {product.Category}"
            printfn "")

    let displayCustomers (customers: Customer list) =
        printfn ""
        printfn "👥 DANH SÁCH KHÁCH HÀNG"
        printfn "========================"
        customers
        |> List.iteri (fun i customer -> 
            let phoneDisplay = customer.Phone |> Option.defaultValue "Không có"
            printfn $"[{i + 1}] {customer.Name}"
            printfn $"    📧 Email: {customer.Email}"
            printfn $"    📞 Điện thoại: {phoneDisplay}"
            printfn $"    🏠 Địa chỉ: {customer.Address}"
            printfn "")

    let displayOrder (order: Order) =
        printfn ""
        printfn "🧾 CHI TIẾT ĐƠN HÀNG"
        printfn "===================="
        let (OrderId orderId) = order.Id
        printfn $"📝 Mã đơn hàng: {orderId}"
        printfn $"👤 Khách hàng: {order.Customer.Name}"
        printfn $"📧 Email: {order.Customer.Email}"
        printfn $"🏠 Địa chỉ: {order.Customer.Address}"
        let phoneDisplay = order.Customer.Phone |> Option.defaultValue "Không có"
        let dateDisplay = order.OrderDate.ToString("dd/MM/yyyy HH:mm")
        printfn $"📞 Điện thoại: {phoneDisplay}"
        printfn $"📅 Ngày đặt: {dateDisplay}"
        printfn ""
        printfn "📦 Danh sách sản phẩm:"
        printfn "----------------------"
        
        order.Items
        |> List.iter (fun item ->
            let lineTotal = item.UnitPrice * decimal item.Quantity
            printfn $"   • {item.Product.Name}"
            printfn $"     Giá: ${item.UnitPrice} x {item.Quantity} = ${lineTotal}")
        
        let statusDisplay = 
            match order.Status with 
            | Processing -> "Đang xử lý" 
            | Pending -> "Chờ xử lý" 
            | Shipped -> "Đã gửi" 
            | Delivered -> "Đã giao" 
            | Cancelled -> "Đã hủy"
        printfn ""
        printfn $"💰 TỔNG CỘNG: ${order.TotalAmount}"
        printfn $"📊 Trạng thái: {statusDisplay}"
        printfn ""

    // Interactive selection functions
    let rec selectCustomer (customers: Customer list) : Customer option =
        displayCustomers customers
        printfn "Chọn khách hàng:"
        printfn $"[1-{customers.Length}] Chọn khách hàng (nhập số)"
        printfn "[Q] Thoát"
        printf "Nhập lựa chọn của bạn: "
        
        let input = Console.ReadLine().ToUpper().Trim()
        
        match input with
        | "Q" -> 
            printfn "👋 Tạm biệt!"
            None
        | numStr when System.Int32.TryParse(numStr) |> fst ->
            let num = System.Int32.Parse(numStr)
            if num >= 1 && num <= customers.Length then
                Some customers.[num - 1]
            else
                printfn "❌ Lựa chọn không hợp lệ!"
                selectCustomer customers
        | _ ->
            printfn "❌ Lựa chọn không hợp lệ!"
            selectCustomer customers

    let rec selectProducts (products: Product list) (selectedItems: (Product * int) list) : (Product * int) list =
        displayProducts products
        printfn "🛒 Giỏ hàng hiện tại:"
        if selectedItems.IsEmpty then
            printfn "   (Trống)"
        else
            selectedItems
            |> List.iter (fun (product, qty) -> 
                printfn $"   - {product.Name} x{qty} = ${product.Price * decimal qty:F2}")
        
        printfn ""
        printfn "Lựa chọn:"
        printfn $"[1-{products.Length}] Chọn sản phẩm (nhập số)"
        printfn "[S] Tìm kiếm sản phẩm"
        printfn "[C] Lọc theo danh mục"
        printfn "[A] Xem tất cả sản phẩm"
        printfn "[D] Hoàn thành chọn sản phẩm"
        printfn "[Q] Thoát"
        printf "Nhập lựa chọn của bạn: "
        
        let input = Console.ReadLine().ToUpper().Trim()
        
        match input with
        | "D" when not selectedItems.IsEmpty -> 
            selectedItems
        | "D" -> 
            printfn "❌ Giỏ hàng trống! Vui lòng chọn ít nhất một sản phẩm."
            selectProducts products selectedItems
        | "S" ->
            printf "🔍 Nhập từ khóa tìm kiếm: "
            let searchQuery = Console.ReadLine().Trim()
            if not (String.IsNullOrEmpty searchQuery) then
                let searchResults = ProductRepository.searchProducts searchQuery products
                if searchResults.IsEmpty then
                    printfn $"❌ Không tìm thấy sản phẩm nào với từ khóa '{searchQuery}'"
                    selectProducts products selectedItems
                else
                    printfn $"🔍 Tìm thấy {searchResults.Length} sản phẩm:"
                    selectProducts searchResults selectedItems
            else
                printfn "❌ Từ khóa không được để trống!"
                selectProducts products selectedItems
        | "C" ->
            let categories = ProductRepository.getCategories products
            printfn "📂 Danh mục có sẵn:"
            categories |> List.iteri (fun i cat -> printfn $"   [{i + 1}] {cat}")
            printf "Chọn danh mục (nhập số): "
            let categoryInput = Console.ReadLine().Trim()
            match System.Int32.TryParse(categoryInput) with
            | (true, num) when num >= 1 && num <= categories.Length ->
                let selectedCategory = categories.[num - 1]
                let filteredProducts = ProductRepository.filterByCategory selectedCategory products
                if filteredProducts.IsEmpty then
                    printfn $"❌ Không có sản phẩm nào trong danh mục '{selectedCategory}'"
                    selectProducts products selectedItems
                else
                    printfn $"📂 Hiển thị {filteredProducts.Length} sản phẩm trong danh mục '{selectedCategory}':"
                    selectProducts filteredProducts selectedItems
            | _ ->
                printfn "❌ Lựa chọn danh mục không hợp lệ!"
                selectProducts products selectedItems
        | "A" ->
            // Load all products again if we were in search/filter mode
            match ProductRepository.loadProductsAsync() |> Async.RunSynchronously with
            | Ok allProducts ->
                printfn $"📋 Hiển thị tất cả {allProducts.Length} sản phẩm:"
                selectProducts allProducts selectedItems
            | Error _ ->
                printfn "❌ Không thể tải danh sách sản phẩm đầy đủ!"
                selectProducts products selectedItems
        | "Q" -> 
            printfn "👋 Tạm biệt!"
            []
        | numStr when System.Int32.TryParse(numStr) |> fst ->
            let num = System.Int32.Parse(numStr)
            if num >= 1 && num <= products.Length then
                let selectedProduct = products.[num - 1]
                printf $"Nhập số lượng cho {selectedProduct.Name}: "
                let qtyInput = Console.ReadLine()
                match System.Int32.TryParse(qtyInput) with
                | (true, qty) when qty > 0 && qty <= selectedProduct.Stock ->
                    let updatedItems = 
                        match selectedItems |> List.tryFind (fun (p, _) -> p.Id = selectedProduct.Id) with
                        | Some (existingProduct, existingQty) ->
                            selectedItems 
                            |> List.map (fun (p, q) -> 
                                if p.Id = selectedProduct.Id then (p, q + qty) else (p, q))
                        | None ->
                            (selectedProduct, qty) :: selectedItems
                    
                    printfn $"✅ Đã thêm {qty} {selectedProduct.Name} vào giỏ hàng"
                    selectProducts products updatedItems
                | (true, qty) when qty > selectedProduct.Stock ->
                    printfn $"❌ Không đủ hàng! Chỉ còn {selectedProduct.Stock} sản phẩm."
                    selectProducts products selectedItems
                | _ ->
                    printfn "❌ Số lượng không hợp lệ!"
                    selectProducts products selectedItems
            else
                printfn "❌ Lựa chọn không hợp lệ!"
                selectProducts products selectedItems
        | _ ->
            printfn "❌ Lựa chọn không hợp lệ!"
            selectProducts products selectedItems

    let createOrder (customer: Customer) (selectedItems: (Product * int) list) : Order =
        let orderItems = 
            selectedItems
            |> List.map (fun (product, quantity) -> {
                Product = product
                Quantity = quantity
                UnitPrice = product.Price
            })

        let totalAmount = 
            orderItems
            |> List.sumBy (fun item -> item.UnitPrice * decimal item.Quantity)

        {
            Id = OrderId (sprintf "ORDER-%s" (DateTime.Now.ToString("yyyyMMdd-HHmmss")))
            Customer = customer
            Items = orderItems
            Status = Processing
            OrderDate = DateTime.Now
            TotalAmount = totalAmount
        }

    let runInteractiveShopAsync () =
        async {
            printfn "🛍️  CHÀO MỪNG ĐẾN CỬA HÀNG F# SHOP!"
            printfn "====================================="
            printfn ""
            
            let! dataResult = loadDataAsync()
            match dataResult with
            | Error err ->
                printfn $"❌ Lỗi khi tải dữ liệu: {ErrorHandling.formatShopError err}"
            | Ok (products, customers) ->
                printfn $"✅ Đã tải {products.Length} sản phẩm và {customers.Length} khách hàng"
                
                // Step 1: Select customer
                match selectCustomer customers with
                | None -> ()
                | Some selectedCustomer ->
                    printfn $"✅ Đã chọn khách hàng: {selectedCustomer.Name}"
                    
                    // Step 2: Select products
                    let selectedItems = selectProducts products []
                    
                    if not selectedItems.IsEmpty then
                        // Step 3: Create and display order
                        let order = createOrder selectedCustomer selectedItems
                        displayOrder order
                        
                        // Step 4: Save to CSV (async)
                        let! saveResult = OrderRepository.saveOrderAsync order
                        match saveResult with
                        | Ok () ->
                            printfn "✅ Đơn hàng đã được lưu vào file CSV thành công!"
                            printfn "📁 Kiểm tra file: Data/orders.csv"
                        | Error err ->
                            printfn $"❌ Lỗi khi lưu đơn hàng: {ErrorHandling.formatShopError err}"
                        
                        printfn "🚚 Chúng tôi sẽ liên hệ để xác nhận và giao hàng."
                    else
                        printfn "❌ Không có đơn hàng nào được tạo."
        }
    
    // Synchronous wrapper for compatibility
    let runInteractiveShop () =
        runInteractiveShopAsync () |> Async.RunSynchronously

    [<EntryPoint>]
    let main args =
        Console.OutputEncoding <- System.Text.Encoding.UTF8
        Console.InputEncoding <- System.Text.Encoding.UTF8
        
        // Ensure Data directory exists
        System.IO.Directory.CreateDirectory("Data") |> ignore
        
        try
            runInteractiveShop ()
            
            printf "\nNhấn phím bất kỳ để thoát..."
            Console.ReadKey() |> ignore
            0
        with
        | ex ->
            Console.WriteLine(sprintf "\n❌ Lỗi: %s" ex.Message)
            1
