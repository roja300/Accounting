require "minitest/autorun"
require "pathname"

class LandingSiteTest < Minitest::Test
  ROOT = Pathname.new(__dir__).join("..").expand_path

  def test_required_markdown_pages_have_front_matter
    %w[index.md getting-started.md guide.md 404.md].each do |relative_path|
      content = ROOT.join(relative_path).read
      assert content.start_with?("---\n"), "#{relative_path} must start with Jekyll front matter"
      assert_operator content.scan(/^---$/).length, :>=, 2, "#{relative_path} must close its front matter"
    end
  end

  def test_internal_page_targets_exist
    all_content = Dir[ROOT.join("**", "*.{md,html}")].map { |path| File.read(path) }.join("\n")
    routes = all_content.scan(/\{\{\s*['\"](\/[a-z0-9\-\/]*\/?)['\"]\s*\|\s*relative_url\s*\}\}/).flatten.uniq
    routes.reject { |route| route.start_with?("/assets/") || route == "/" }.each do |route|
      expected = ROOT.join("#{route.delete_prefix("/").delete_suffix("/")}.md")
      assert expected.file?, "#{route} should resolve to a Markdown page"
    end
  end

  def test_referenced_assets_exist
    all_content = Dir[ROOT.join("**", "*.{md,html}")].map { |path| File.read(path) }.join("\n")
    assets = all_content.scan(/['\"](\/assets\/[a-zA-Z0-9._\-\/]+)['\"]/).flatten.uniq
    refute_empty assets
    assets.each do |asset|
      source = ROOT.join(asset.delete_prefix("/"))
      source = source.sub_ext(".scss") if source.extname == ".css" && !source.file?
      assert source.file?, "#{asset} should have a source file"
    end
  end

  def test_site_links_to_the_source_repository
    config = ROOT.join("_config.yml").read
    assert_includes config, "github_url: \"https://github.com/roja300/Accounting\""
    assert_includes ROOT.join("index.md").read, "site.github_url"
  end
end
